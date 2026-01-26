namespace Sql.SemanticSearch.Core.Search;

public record SearchResultItem()
{
    public required string ArxivId { get; init; }

    public float Distance { get; set; }

    public required string Title { get; set; }

    public string? Summary { get; set; }

    public string? Comments { get; set; }

    public DocumentMetadata? Metadata { get; set; }

    public Uri? PdfUri { get; set; }

    public DateTime? Published { get; set; }
}
