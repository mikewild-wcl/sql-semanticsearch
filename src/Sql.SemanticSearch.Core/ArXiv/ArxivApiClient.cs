using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Sql.SemanticSearch.Core.ArXiv;

public class ArxivApiClient(
    HttpClient httpClient,
    ILogger<ArxivApiClient> logger) : IArxivApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    //TODO: Replace console logging with calls to logger
#pragma warning disable CA1823 // Avoid unused private fields
    private readonly ILogger<ArxivApiClient> _logger = logger;
#pragma warning restore CA1823 // Avoid unused private fields

    [SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "The HttpClient will have a base uri set.")]
    public async Task<ArxivPaper> GetPaperInfo(string arxivId)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(arxivId);

            // Clean the arXiv ID (remove version if present)
            arxivId = arxivId.Replace("arXiv:", "", StringComparison.InvariantCultureIgnoreCase).Split('v')[0];

            // Build the API query URL - we can do better and use a list, but need to be careful with max items
            string queryUrl = $"query?id_list={arxivId}";

            Console.WriteLine($"Fetching paper info from: {queryUrl}");

            var response = await _httpClient.GetAsync(queryUrl);
            response.EnsureSuccessStatusCode();

            string xmlContent = await response.Content.ReadAsStringAsync();

            var doc = XDocument.Parse(xmlContent);

            // Define all relevant namespaces
            XNamespace atom = "http://www.w3.org/2005/Atom";
            XNamespace arxiv = "http://arxiv.org/schemas/atom";
            XNamespace opensearch = "http://a9.com/-/spec/opensearch/1.1/";

            var entry = doc.Descendants(atom + "entry").FirstOrDefault();
            if (entry == null)
            {
                throw new ArxivPaperNotFoundException(arxivId);
            }

            var pdfLink = entry.Descendants(atom + "link")
                .FirstOrDefault(l => l.Attribute("title")?.Value == "pdf");

            var pdfUrl = pdfLink?.Attribute("href")?.Value;
            var published = DateTime.TryParse(entry.Element(atom + "published")?.Value, out var publishedDate) ? publishedDate : DateTime.MinValue;

            var paper = new ArxivPaper(entry.Element(atom + "id")?.Value, entry.Element(atom + "title")?.Value?.Trim())
            {
                PdfUri = pdfUrl is not null ? new Uri(pdfUrl) : null,
                Summary = entry.Element(atom + "summary")?.Value?.Trim(),
                Published = published,
                Authors = entry.Descendants(atom + "author")
                    .Select(a => a.Element(atom + "name")?.Value)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Select(name => name!)
                    .ToList()
                    ?? []
            };

            return paper;
        }
        catch (HttpRequestException ex)
        {
            throw new ArxivApiException($"Error fetching paper info: {ex.Message}", ex);
        }
    }

    public async Task<MemoryStream> DownloadPdfToMemoryStream(Uri pdfUri)
    {
        try
        {
            Console.WriteLine($"Downloading PDF from: {pdfUri}");

            HttpResponseMessage response = await _httpClient.GetAsync(pdfUri);
            response.EnsureSuccessStatusCode();

            // Read the content as byte array
            byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

            // Create a MemoryStream from the bytes
            MemoryStream memoryStream = new MemoryStream(pdfBytes);

            Console.WriteLine($"PDF downloaded successfully. Size: {pdfBytes.Length} bytes");

            return memoryStream;
        }
        catch (HttpRequestException ex)
        {
            throw new ArxivPdfDownloadException($"Error downloading PDF: {ex.Message}", ex);
        }
    }

    public async Task<(ArxivPaper paper, MemoryStream pdfStream)> GetPaperWithPdf(string arxivId)
    {
        var paper = await GetPaperInfo(arxivId);

        if(paper.PdfUri is null)
        {
            throw new ArxivPdfDownloadException("PDF URL is not available for this paper.");
        }

        var pdfStream = await DownloadPdfToMemoryStream(paper.PdfUri);
        return (paper, pdfStream);
    }
}
