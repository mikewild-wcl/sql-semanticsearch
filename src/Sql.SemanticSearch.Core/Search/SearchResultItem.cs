namespace Sql.SemanticSearch.Core.Search;

public class SearchResultItem
{
    public required string ArxivId { get; set; }

    public float Distance { get; set; }

    public required string Title { get; set; }

    public string Summary { get; set; }

    public string? Comments { get; set; }

    public string? Metadata { get; set; }

    public Uri? PdfUri { get; set; }

    public DateTime? Published { get; set; }
}
