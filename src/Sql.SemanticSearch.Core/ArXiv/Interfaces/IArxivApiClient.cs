using System.Threading;
namespace Sql.SemanticSearch.Core.ArXiv.Interfaces;

public interface IArxivApiClient
{
    Task<ArxivPaper> GetPaperInfo(string arxivId);

    Task<MemoryStream> DownloadPdfToMemoryStream(Uri pdfUri);

    Task<(ArxivPaper paper, MemoryStream pdfStream)> GetPaperWithPdf(string arxivId);
}
