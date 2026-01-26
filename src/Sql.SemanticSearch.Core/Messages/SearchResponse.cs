using Sql.SemanticSearch.Core.Search;

namespace Sql.SemanticSearch.Core.Messages;

public record SearchResponse(IReadOnlyCollection<SearchResultItem> Items);

