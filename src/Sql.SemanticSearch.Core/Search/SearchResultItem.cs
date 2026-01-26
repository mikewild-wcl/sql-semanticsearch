namespace Sql.SemanticSearch.Core.Search;

public record SearchResultItem()
{
    public required string ArxivId { get; init; }

    public float Distance { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Comments { get; init; }

    public DocumentMetadata? Metadata { get; init; }

    public Uri? PdfUri { get; init; }

    public DateTime? Published { get; init; }
}
