using Sql.SemanticSearch.Core.Messages;

namespace Sql.SemanticSearch.Core.Search.Interfaces;

public interface ISearchService
{
    Task<IEnumerable<SearchResultItem>> Search(SearchRequest searchRequest, CancellationToken cancellationToken = default);
}
