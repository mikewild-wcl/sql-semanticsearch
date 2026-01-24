namespace Sql.SemanticSearch.Core.ArXiv.Interfaces;

public interface IArxivApiClient
{
    IAsyncEnumerable<ArxivPaper> GetPapers(IEnumerable<string> arxivIds, int maxResults = 10, CancellationToken cancellationToken = default);

    Task<MemoryStream> DownloadPdfToMemoryStream(Uri pdfUri, CancellationToken cancellationToken = default);

    Task<(ArxivPaper paper, MemoryStream pdfStream)> GetPaperWithPdf(string arxivId, CancellationToken cancellationToken = default);
}
