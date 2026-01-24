using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
using Sql.SemanticSearch.Core.ArXiv.Extensions;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sql.SemanticSearch.Core.ArXiv;

public class ArxivApiClient(
    HttpClient httpClient,
    ILogger<ArxivApiClient> logger) : IArxivApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<ArxivApiClient> _logger = logger;

    private static readonly Action<ILogger, string, Exception?> _logFetchingPaperInfo =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(0, nameof(ArxivApiClient)),
            "Fetching paper info from: {QueryUrl}");

    private static readonly Action<ILogger, Uri, Exception?> _logDownloadingPdf =
        LoggerMessage.Define<Uri>(
            LogLevel.Information,
            new EventId(0, nameof(ArxivApiClient)),
            "Downloading PDF from: {PdfUri}");

    private static readonly Action<ILogger, long, Exception?> _logPdfDownloadedSuccessfully =
        LoggerMessage.Define<long>(
            LogLevel.Information,
            new EventId(0, nameof(ArxivApiClient)),
            "PDF downloaded successfully. Size: {PdfSize} bytes");

    [SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "The HttpClient will have a base uri set.")]
    public async IAsyncEnumerable<ArxivPaper> GetPapers(
        IEnumerable<string> arxivIds,
        int maxResults = 10,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        // Clean the arXiv ID (remove version if present)
        var idList = string.Join(',', arxivIds
            .Select(id => id.Replace("arXiv:", "", StringComparison.InvariantCultureIgnoreCase)
                            .Split('v')
                            ?.FirstOrDefault()
                            ?.Trim()));

        if (string.IsNullOrWhiteSpace(idList))
        {
            yield break;
        }

        int start = 0;
        int processed = 0;

        while (true)
        {
            string query = $"query?id_list={idList}&start={start}&max_results={maxResults}";

            _logFetchingPaperInfo(_logger, new Uri(_httpClient.BaseAddress!, query).AbsoluteUri, null);

            Console.WriteLine($"[Fetching] start={start}, max={maxResults}");

            var response = await _httpClient.GetAsync(query, cancellationToken);
            response.EnsureSuccessStatusCode();

            string xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var doc = XDocument.Parse(xmlContent);

            var entries = doc.Descendants(ArxivNamespace.Atom + "entry").ToList();

            if (entries.Count == 0)
            {
                yield break;
            }

            foreach (var entry in entries)
            {
                var paper = entry.ToArxivPaper();
                yield return paper;
            }

            start += maxResults;
            processed += entries.Count;

            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            if (processed < arxivIds.Count())
            {
                await Task.Delay(3000, cancellationToken); // Rate limiting
            }
        }
    }

    public async Task<MemoryStream> DownloadPdfToMemoryStream(Uri pdfUri, CancellationToken cancellationToken = default)
    {
        try
        {
            _logDownloadingPdf(_logger, pdfUri, null);

            var response = await _httpClient.GetAsync(pdfUri, cancellationToken: cancellationToken);
            response.EnsureSuccessStatusCode();

            byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var memoryStream = new MemoryStream(pdfBytes);

            _logPdfDownloadedSuccessfully(_logger, pdfBytes.Length, null);

            return memoryStream;
        }
        catch (HttpRequestException ex)
        {
            throw new ArxivPdfDownloadException($"Error downloading PDF: {ex.Message}", ex);
        }
    }

    public async Task<(ArxivPaper paper, MemoryStream pdfStream)> GetPaperWithPdf(string arxivId, CancellationToken cancellationToken = default)
    {
        var paper = await GetPapers([arxivId], cancellationToken: cancellationToken).FirstOrDefaultAsync(cancellationToken);

        if (paper.PdfUri is null)
        {
            throw new ArxivPdfDownloadException("PDF URL is not available for this paper.");
        }

        var pdfStream = await DownloadPdfToMemoryStream(paper.PdfUri, cancellationToken: cancellationToken);
        return (paper, pdfStream);
    }
}
