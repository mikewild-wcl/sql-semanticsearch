namespace Sql.SemanticSearch.Core.Search;

public record DocumentMetadata()
{
    public IReadOnlyCollection<string> Authors { get; init; } = [];

    public IReadOnlyCollection<string> Categories { get; init; } = [];
}
