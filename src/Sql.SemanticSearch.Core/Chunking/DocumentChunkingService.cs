using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
using Sql.SemanticSearch.Core.Chunking.Interfaces;

namespace Sql.SemanticSearch.Core.Chunking;

public class DocumentChunkingService(
    IDocumentReader reader,
    IngestionChunkWriter<string> chunkWriter,
    ILogger<DocumentChunkingService> logger) : IDocumentChunkingService
{
    private readonly IDocumentReader _reader = reader;
    private readonly IngestionChunkWriter<string> _chunkWriter = chunkWriter;
    private readonly ILogger<DocumentChunkingService> _logger = logger;

    private static readonly Action<ILogger, int, Uri, Exception?> _logPdfDownloadFailed =
        LoggerMessage.Define<int, Uri>(
            LogLevel.Warning,
            new EventId(1, nameof(DocumentChunkingService)),
            "Failed to download PDF for document {DocumentId} from {Uri}. Falling back to PdfPig.");

    public async Task IndexDocument(DatabaseDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var chunker = CreateChunker();
        var ingestionDocument = await LoadIngestionDocumentAsync(document, cancellationToken);
        await _chunkWriter.WriteAsync(chunker.ProcessAsync(ingestionDocument, cancellationToken), cancellationToken);
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
}
