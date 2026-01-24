using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Sql.SemanticSearch.Core.ArXiv.Extensions;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Extension members don't need to be static")]
internal static class ArxivElementExtensionss
{
    extension(XElement? entry)
    {
        public ArxivPaper ToArxivPaper()
        {
            var entryId = entry.Element(ArxivNamespace.Atom + "id")?.Value;
            var id = entryId.ToShortId();

            var pdfLink = entry.Descendants(ArxivNamespace.Atom + "link")
                .FirstOrDefault(l => l.Attribute("title")?.Value == "pdf");

            var pdfUrl = pdfLink?.Attribute("href")?.Value;
            var published = DateTime.TryParse(entry.Element(ArxivNamespace.Atom + "published")?.Value, out var publishedDate) ? publishedDate : DateTime.MinValue;

            return new ArxivPaper(id, entry.Element(ArxivNamespace.Atom + "title")?.Value?.Trim())
            {
                PdfUri = pdfUrl is not null ? new Uri(pdfUrl) : null,
                Summary = entry.Element(ArxivNamespace.Atom + "summary")?.Value?.Trim() ?? string.Empty,
                Comments = entry.Element(ArxivNamespace.Atom + "comment")?.Value?.Trim(),
                Published = published,
                Authors = entry.Descendants(ArxivNamespace.Atom + "author")
                    .Select(a => a.Element(ArxivNamespace.Atom + "name")?.Value)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Select(name => name!)
                    .ToList()
                    ?? [],
                Categories = entry.Descendants(ArxivNamespace.Atom + "category")
                    .Select(c => c.Attribute("term")?.Value)
                        .Where(term => !string.IsNullOrEmpty(term))
                        .Select(term => term!)
                        .ToList()
                        ?? []
            };
        }
    }
}

