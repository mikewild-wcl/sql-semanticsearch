using Microsoft.Extensions.DataIngestion;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Chunking.Interfaces;
using UglyToad.PdfPig;

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

    public async Task<IngestionDocument> ReadWithPdfPig(Uri uri, string documentIdentifier, string? mediaType = null, CancellationToken cancellationToken = default)
    {
        using var stream = await _arxivApiClient.DownloadPdfToMemoryStream(uri, cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);

        using var pdfDocument = PdfDocument.Open(stream);

        var fullText = string.Join(
            Environment.NewLine + Environment.NewLine,
            pdfDocument.GetPages().Select(page => page.Text));

        var rootSection = new IngestionDocumentSection(fullText);

        foreach (var page in pdfDocument.GetPages())
        {
            var pageText = page.Text;
            if (string.IsNullOrWhiteSpace(pageText))
            {
                continue;
            }

            rootSection.Elements.Add(new IngestionDocumentParagraph(pageText)
            {
                Text = pageText,
            });
        }

        return new IngestionDocument(documentIdentifier)
        {
            Sections = { rootSection }
        };
    }
}
