using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search.Interfaces;

namespace Sql.SemanticSearch.Core.Search;

public class SearchService(
    IDatabaseConnection databaseConnection,
    AISettings aiSettings,
    ILogger<SearchService> logger) : ISearchService
{
    //TODO: Remove unused warnings when implemented
#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable CA1823 // Avoid unused private fields
    private readonly IDatabaseConnection _databaseConnection = databaseConnection;
    private readonly AISettings _aiSettings = aiSettings;
    private readonly ILogger<SearchService> _logger = logger;
#pragma warning restore S1144 // Unused private types or members should be removed
#pragma warning restore CA1823 // Avoid unused private fields

    public async Task<IEnumerable<string>> Search(SearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        return ["results"];
    }
}
