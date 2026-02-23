using Microsoft.Extensions.DataIngestion;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Chunking.Interfaces;

namespace Sql.SemanticSearch.Core.Chunking;

public class PdfDocumentReader(
    IArxivApiClient arxivApiClient,
    MarkItDownMcpReader markItDownMcp) : IDocumentReader
{
    private readonly IArxivApiClient _arxivApiClient = arxivApiClient;
    private readonly  MarkItDownMcpReader _markItDownMcp = markItDownMcp;
    
    public async Task<IngestionDocument> Read(Uri uri, string documentIdentifier, string? mediaType = null, CancellationToken cancellationToken = default)
    {
        using var stream = await _arxivApiClient.DownloadPdfToMemoryStream(uri, cancellationToken);

        stream.Seek(0, SeekOrigin.Begin);

        mediaType = mediaType ?? "application/pdf";
        var ingestionDocument = await _markItDownMcp.ReadAsync(stream, documentIdentifier, mediaType, cancellationToken);

        return ingestionDocument;
    }
}
