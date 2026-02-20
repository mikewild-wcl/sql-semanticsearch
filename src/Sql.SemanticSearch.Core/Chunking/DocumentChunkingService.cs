using System.Globalization;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Chunking.Interfaces;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;

namespace Sql.SemanticSearch.Core.Chunking;

public class DocumentChunkingService(
    IArxivApiClient arxivApiClient,
    IDatabaseConnection databaseConnection,
    AISettings aiSettings,
    ILogger<DocumentChunkingService> logger) : IDocumentChunkingService
{
    private readonly IArxivApiClient _arxivApiClient = arxivApiClient;
    private readonly IDatabaseConnection _databaseConnection = databaseConnection;
    private readonly AISettings _aiSettings = aiSettings;
    private readonly ILogger<DocumentChunkingService> _logger = logger;

    // HeaderChunker holds a StringBuilder per instance and is not thread-safe; keep as instance field.
    private readonly HeaderChunker _chunker = CreateChunker();

    // MarkItDownReader is stateless (shells out to the markitdown CLI); safe to share.
    private static readonly MarkItDownReader _reader = new();

    private static readonly Action<ILogger, int, int, Exception?> _logChunkSaved =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(0, nameof(DocumentChunkingService)),
            "Saved chunk {ChunkId} for document {DocumentId}.");

    public async Task IndexDocument(DatabaseDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        await DeleteExistingChunks(document.Id);

        var ingestionDocument = await LoadIngestionDocumentAsync(document, cancellationToken);
        await foreach (var chunk in _chunker.ProcessAsync(ingestionDocument, cancellationToken))
        {
            var chunkId = await SaveDocumentChunk(document.Id, chunk.Content);
            await SaveDocumentChunkEmbedding(chunkId);
            _logChunkSaved(_logger, chunkId, document.Id, null);
        }
    }

    private async Task<IngestionDocument> LoadIngestionDocumentAsync(DatabaseDocument document, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(document.PdfUri, UriKind.Absolute, out var pdfUri))
        {
            throw new ArxivPdfDownloadException($"Cannot download PDF for document {document.Id}: invalid or missing URI.");
        }

        using var pdfStream = await _arxivApiClient.DownloadPdfToMemoryStream(pdfUri, cancellationToken);
        return await _reader.ReadAsync(pdfStream, document.ArxivId ?? document.Id.ToString(CultureInfo.InvariantCulture), "application/pdf", cancellationToken);
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

    private async Task DeleteExistingChunks(int documentId) =>
        await _databaseConnection.ExecuteAsync(
            """
            DELETE FROM dbo.DocumentChunkEmbeddings
            WHERE [Id] IN (SELECT [Id] FROM dbo.DocumentChunks WHERE [DocumentId] = @DocumentId);

            DELETE FROM dbo.DocumentChunks
            WHERE [DocumentId] = @DocumentId;
            """,
            new { DocumentId = documentId });

    private async Task<int> SaveDocumentChunk(int documentId, string content) =>
        await _databaseConnection.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.DocumentChunks ([DocumentId], [Content])
            VALUES (@DocumentId, @Content);

            SELECT CAST(SCOPE_IDENTITY() as int);
            """,
            new { DocumentId = documentId, Content = content });

    /* Note: Embedding model is *NOT* a SQL injection risk, it must be hard-coded so we have to use the settings value. */
    private async Task SaveDocumentChunkEmbedding(int chunkId) =>
        await _databaseConnection.ExecuteAsync(
            $"""
            INSERT INTO dbo.DocumentChunkEmbeddings ([Id], [Embedding])
            SELECT @Id, AI_GENERATE_EMBEDDINGS([Content] USE MODEL {_aiSettings.ExternalEmbeddingModel})
            FROM dbo.DocumentChunks
            WHERE [Id] = @Id;
            """,
            new { Id = chunkId });
}
