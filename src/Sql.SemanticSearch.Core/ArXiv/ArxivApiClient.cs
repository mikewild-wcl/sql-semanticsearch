using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
using Sql.SemanticSearch.Core.ArXiv.Extensions;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Linq;
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
    public async IAsyncEnumerable<ArxivPaper> GetPaperInfo(IEnumerable<string> arxivIds, int maxItems = 10)
    {
        //try
        //{
        //ArgumentException.ThrowIfNullOrEmpty(arxivId);
        var start = 0;
        do  //Loop will 
        {
            //Don't query more than max items. We aren't using arxiv paging in this code, just doing our own thing
            var batch = arxivIds.Skip(start).Take(maxItems);
            await foreach (var paper in GetPaperInfo(batch, start, maxItems))
            {
                yield return paper;
            }

            start += maxItems;
        } while (start < maxItems);
    }

    public async Task<MemoryStream> DownloadPdfToMemoryStream(Uri pdfUri)
    {
        try
        {
            _logDownloadingPdf(_logger, pdfUri, null);

            HttpResponseMessage response = await _httpClient.GetAsync(pdfUri);
            response.EnsureSuccessStatusCode();

            // Read the content as byte array
            byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

            // Create a MemoryStream from the bytes
            MemoryStream memoryStream = new MemoryStream(pdfBytes);

            _logPdfDownloadedSuccessfully(_logger, pdfBytes.Length, null);

            return memoryStream;
        }
        catch (HttpRequestException ex)
        {
            throw new ArxivPdfDownloadException($"Error downloading PDF: {ex.Message}", ex);
        }
    }

    public async Task<(ArxivPaper paper, MemoryStream pdfStream)> GetPaperWithPdf(string arxivId)
    {
        var paper = await GetPaperInfo([arxivId]).FirstOrDefaultAsync();

        if (paper.PdfUri is null)
        {
            throw new ArxivPdfDownloadException("PDF URL is not available for this paper.");
        }

        var pdfStream = await DownloadPdfToMemoryStream(paper.PdfUri);
        return (paper, pdfStream);
    }

    private async IAsyncEnumerable<ArxivPaper> GetPaperInfo(IEnumerable<string> arxivIds, int start, int maxItems)
    {
        //try
        //{
        //ArgumentException.ThrowIfNullOrEmpty(arxivId);

        /*
         * Needs a loop that calls the rest of this code with start and maxResults until we have enough items or there are no more items.
         https://info.arxiv.org/help/api/user-manual.html#paging
         */

        // Clean the arXiv ID (remove version if present)
        var idList = string.Join(',', arxivIds
            .Select(id =>
            id.Replace("arXiv:", "", StringComparison.InvariantCultureIgnoreCase).Split('v')[0])
            );
        //arxivId = arxivId.Replace("arXiv:", "", StringComparison.InvariantCultureIgnoreCase).Split('v')[0];

        // Build the API query URL - we can do better and use a list, but need to be careful with max items
        string queryUrl = $"query?id_list={idList}&start={start}&max_results={maxItems}";

        _logFetchingPaperInfo(_logger, queryUrl, null);

#pragma warning disable CA2234 // Pass system uri objects instead of strings
        var response = await _httpClient.GetAsync(queryUrl);
#pragma warning restore CA2234 // Pass system uri objects instead of strings
        response.EnsureSuccessStatusCode();

        string xmlContent = await response.Content.ReadAsStringAsync();

        var doc = XDocument.Parse(xmlContent);

        // Define all relevant namespaces
        XNamespace atom = "http://www.w3.org/2005/Atom";
        XNamespace arxiv = "http://arxiv.org/schemas/atom";
        XNamespace opensearch = "http://a9.com/-/spec/opensearch/1.1/";

        //var entry = doc.Descendants(atom + "entry").FirstOrDefault();
        //if (entry == null)
        //{
        //    throw new ArxivPaperNotFoundException(arxivId);
        //}

        foreach (var entry in doc.Descendants(atom + "entry"))
        {
            var entryId = entry.Element(atom + "id")?.Value;
            var id = entryId.ToShortId();

            var pdfLink = entry.Descendants(atom + "link")
                .FirstOrDefault(l => l.Attribute("title")?.Value == "pdf");

            var pdfUrl = pdfLink?.Attribute("href")?.Value;
            var published = DateTime.TryParse(entry.Element(atom + "published")?.Value, out var publishedDate) ? publishedDate : DateTime.MinValue;

            var paper = new ArxivPaper(id, entry.Element(atom + "title")?.Value?.Trim())
            {
                PdfUri = pdfUrl is not null ? new Uri(pdfUrl) : null,
                Summary = entry.Element(atom + "summary")?.Value?.Trim() ?? string.Empty,
                Comments = entry.Element(atom + "comment")?.Value?.Trim(),
                Published = published,
                Authors = entry.Descendants(atom + "author")
                    .Select(a => a.Element(atom + "name")?.Value)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Select(name => name!)
                    .ToList()
                    ?? [],
                Categories = entry.Descendants(atom + "category")
                    .Select(c => c.Attribute("term")?.Value)
                        .Where(term => !string.IsNullOrEmpty(term))
                        .Select(term => term!)
                        .ToList()
                        ?? []
            };

            yield return paper;
        }
        //}
        //catch (HttpRequestException ex)
        //{
        //    throw new ArxivApiException($"Error fetching paper info: {ex.Message}", ex);
        //}
    }
}
