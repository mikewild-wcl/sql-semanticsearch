using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Sql.SemanticSearch.Core.ArXiv.Extensions;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Extension members don't need to be static")]
internal static class ArxivElementExtensionss
{
    // Define all relevant namespaces
    private static XNamespace atom = "http://www.w3.org/2005/Atom";
    //static XNamespace arxiv = "http://arxiv.org/schemas/atom";
    //static XNamespace opensearch = "http://a9.com/-/spec/opensearch/1.1/";

    extension(XElement? entry)
    {
        public ArxivPaper ToArxivPaper()
        {
            var entryId = entry.Element(atom + "id")?.Value;
            var id = entryId.ToShortId();

            var pdfLink = entry.Descendants(atom + "link")
                .FirstOrDefault(l => l.Attribute("title")?.Value == "pdf");

            var pdfUrl = pdfLink?.Attribute("href")?.Value;
            var published = DateTime.TryParse(entry.Element(atom + "published")?.Value, out var publishedDate) ? publishedDate : DateTime.MinValue;

            return new ArxivPaper(id, entry.Element(atom + "title")?.Value?.Trim())
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
        }
    }
}

