namespace Sql.SemanticSearch.Core.ArXiv.Interfaces;

public interface IArxivApiClient
{
    IAsyncEnumerable<ArxivPaper> GetPaperInfo(IEnumerable<string> arxivIds, int maxResults = 10);

    IAsyncEnumerable<ArxivPaper> GetPapersAsync(IEnumerable<string> arxivIds, int maxResults = 10, CancellationToken cancellationToken = default);

    Task<MemoryStream> DownloadPdfToMemoryStream(Uri pdfUri);

    Task<(ArxivPaper paper, MemoryStream pdfStream)> GetPaperWithPdf(string arxivId);
}
