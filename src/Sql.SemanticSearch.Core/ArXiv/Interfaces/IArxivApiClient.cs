namespace Sql.SemanticSearch.Core.ArXiv.Interfaces;

public interface IArxivApiClient
{
    IAsyncEnumerable<ArxivPaper> GetPaperInfo(IEnumerable<string> arxivIds, int maxItems = 10);

    Task<MemoryStream> DownloadPdfToMemoryStream(Uri pdfUri);

    Task<(ArxivPaper paper, MemoryStream pdfStream)> GetPaperWithPdf(string arxivId);
}
