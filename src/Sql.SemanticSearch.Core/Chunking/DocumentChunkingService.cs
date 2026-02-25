using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using Polly.Registry;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
using Sql.SemanticSearch.Core.Chunking.Interfaces;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Shared;

namespace Sql.SemanticSearch.Core.Chunking;

public class DocumentChunkingService(
    IDatabaseConnection databaseConnection,
    IDocumentReader reader,
    ResiliencePipelineProvider<string> resiliencePipelineProvider,
    AISettings aiSettings,
    ILogger<DocumentChunkingService> logger) : IDocumentChunkingService
{
    private readonly IDatabaseConnection _databaseConnection = databaseConnection;
    private readonly IDocumentReader _reader = reader;
    private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider = resiliencePipelineProvider;
    private readonly AISettings _aiSettings = aiSettings;
    private readonly ILogger<DocumentChunkingService> _logger = logger;

    // HeaderChunker holds a StringBuilder per instance and is not thread-safe; keep as instance field.
    private readonly HeaderChunker _chunker = CreateChunker();

    // MarkItDownReader is stateless (shells out to the markitdown CLI); safe to share.
    //private static readonly MarkItDownReader _reader = new();

    private static readonly Action<ILogger, int, int, Exception?> _logChunkSaved =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(0, nameof(DocumentChunkingService)),
            "Saved chunk {ChunkId} for document {DocumentId}.");

    private static readonly Action<ILogger, int, Uri, Exception?> _logPdfDownloadFailed =
        LoggerMessage.Define<int, Uri>(
            LogLevel.Warning,
            new EventId(1, nameof(DocumentChunkingService)),
            "Failed to download PDF for document {DocumentId} from {Uri}. Falling back to PdfPig.");

    public async Task IndexDocument(DatabaseDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        await DeleteExistingChunks(document.Id, cancellationToken);

        var ingestionDocument = await LoadIngestionDocumentAsync(document, cancellationToken);
        await foreach (var chunk in _chunker.ProcessAsync(ingestionDocument, cancellationToken))
        {
            await WriteDocumentChunk(document.Id, chunk.Content, cancellationToken);
        }
    }

    private async Task<IngestionDocument> LoadIngestionDocumentAsync(DatabaseDocument document, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(document.PdfUri, UriKind.Absolute, out var uri))
        {
            throw new ArxivPdfDownloadException($"Cannot download PDF for document {document.Id}: invalid or missing URI.");
        }

        try
        {
            return await _reader.Read(uri, document.ArxivId, default, cancellationToken: cancellationToken);
        }
#pragma warning disable CA1031 // Intentional catch-all: any failure from MarkItDown should fall back to PdfPig
        catch (Exception ex)
#pragma warning restore CA1031
        {
            // fallback to PdfPig
            _logPdfDownloadFailed(_logger, document.Id, uri, ex);

            return await _reader.ReadWithPdfPig(uri, document.ArxivId, cancellationToken: cancellationToken);
        }
    }

    private static HeaderChunker CreateChunker()
    {
        var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        return new HeaderChunker(new IngestionChunkerOptions(tokenizer)
        {
            MaxTokensPerChunk = 2000,
            OverlapTokens = 0
        });
    }

    private async Task DeleteExistingChunks(int documentId, CancellationToken cancellationToken)
    {
        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline(ResiliencePipelineNames.SqlServerRetry);
        await resiliencePipeline.ExecuteAsync(
            async context =>
            {
                await DeleteChunks(documentId, cancellationToken);
            },
            cancellationToken);
    }

    private async Task DeleteChunks(int documentId, CancellationToken cancellationToken) =>
        await _databaseConnection.ExecuteAsync(
            """
            DELETE FROM dbo.DocumentChunkEmbeddings
            WHERE [Id] IN (SELECT [Id] FROM dbo.DocumentChunks WHERE [DocumentId] = @DocumentId);

            DELETE FROM dbo.DocumentChunks
            WHERE [DocumentId] = @DocumentId;
            """,
            new { DocumentId = documentId });


    private async Task WriteDocumentChunk(int documentId, string content, CancellationToken cancellationToken)
    {
        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline(ResiliencePipelineNames.SqlServerRetry);
        await resiliencePipeline.ExecuteAsync(
            async context =>
            {
                using var connection = _databaseConnection.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                //await DeleteExistingDocumentIfExists(paper.Id, transaction);

                var chunkId = await SaveDocumentChunk(documentId, content, transaction);
                await SaveDocumentChunkEmbedding(chunkId, transaction);
                _logChunkSaved(_logger, chunkId, documentId, null);

                transaction.Commit();
            },
           cancellationToken);

    }


    private async Task<int> SaveDocumentChunk(int documentId, string content, System.Data.IDbTransaction transaction) =>
        await _databaseConnection.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.DocumentChunks ([DocumentId], [Content])
            VALUES (@DocumentId, @Content);

            SELECT CAST(SCOPE_IDENTITY() as int);
            """,
            new { DocumentId = documentId, Content = content },
            transaction: transaction);

    /* Note: Embedding model is *NOT* a SQL injection risk, it must be hard-coded so we have to use the settings value. */
    private async Task SaveDocumentChunkEmbedding(int chunkId, System.Data.IDbTransaction transaction) =>
        await _databaseConnection.ExecuteAsync(
            $"""
            INSERT INTO dbo.DocumentChunkEmbeddings ([Id], [Embedding])
            SELECT @Id, AI_GENERATE_EMBEDDINGS([Content] USE MODEL {_aiSettings.ExternalEmbeddingModel})
            FROM dbo.DocumentChunks
            WHERE [Id] = @Id;
            """,
            new { Id = chunkId },
            transaction: transaction);
}
