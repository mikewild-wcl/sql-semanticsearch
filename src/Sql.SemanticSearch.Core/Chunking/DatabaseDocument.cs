namespace Sql.SemanticSearch.Core.Chunking;

public record DatabaseDocument(int Id)
{
    public string ArxivId { get; init; }

    public string Title { get; init; }

    public string Summary { get; init; }

    public string? Comments { get; init; }

    public string? Metadata { get; init; }

#pragma warning disable CA1056 // URI-like properties should not be strings
    public string? PdfUri { get; init; }
#pragma warning restore CA1056 // URI-like properties should not be strings

    public DateTime? Published { get; init; }
}
