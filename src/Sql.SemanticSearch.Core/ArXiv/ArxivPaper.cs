namespace Sql.SemanticSearch.Core.ArXiv;

public record ArxivPaper(string Id, string Title)
{
    public required string Summary { get; init; }

    public string? Comments { get; init; }

    public required Uri? PdfUri { get; init; }

    public DateTime Published { get; init; }

    public IReadOnlyCollection<string> Authors { get; init; } = [];

    public IReadOnlyCollection<string> Categories { get; init; } = [];
}
