using ModelContextProtocol.Server;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search.Interfaces;
using Sql.SemanticSearch.Core.Serialization;
using System.ComponentModel;
using System.Text.Json;

namespace Sql.SemanticSearch.Api.Mcp;

[McpServerToolType]
public sealed class SearchTools(ISearchService searchService)
{
    private readonly ISearchService _searchService = searchService;

    [McpServerTool(Name = "semantic_search")]
    [Description("Search for academic papers and documents using semantic similarity. Returns the most relevant documents matching the query as JSON.")]
    public async Task<string> SearchDocuments(
        [Description("The natural language search query to find relevant documents.")] string query,
        [Description("The maximum number of results to return. Defaults to 5.")] int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var searchRequest = new SearchRequest
        {
            Query = query,
            Top = topK
        };

        var results = await _searchService.Search(searchRequest, cancellationToken);
        var response = new SearchResponse([.. results]);
        
        var s1 = JsonSerializer.Serialize(results, SerializerOptions.DefaultWebSerializerOptions);
        var s2 = JsonSerializer.Serialize(response, SerializerOptions.DefaultWebSerializerOptions);
        var s3 = JsonSerializer.Serialize(response, SerializerOptions.CamelCaseSerializerOptions);

        return JsonSerializer.Serialize(response, SerializerOptions.DefaultWebSerializerOptions);
    }
}
