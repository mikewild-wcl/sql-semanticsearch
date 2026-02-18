using ModelContextProtocol.Server;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Search.Interfaces;
using System.ComponentModel;

namespace Sql.SemanticSearch.Api.Mcp;

[McpServerToolType]
public sealed class SearchTools(ISearchService searchService)
{
    private readonly ISearchService _searchService = searchService;

    [McpServerTool(Name = "semantic_search", UseStructuredContent = true)]
    [Description("Search for academic papers and documents using semantic similarity. Returns a structured list of the most relevant documents matching the query.")]
    public async Task<List<SearchToolResponseItem>> SearchDocuments(
        [Description("The natural language search query to find relevant documents.")] string query,
        [Description("The maximum number of results to return. Defaults to 5.")] int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var results = await _searchService.Search(
             new SearchRequest
             {
                 Query = query,
                 Top = topK
             },
             cancellationToken);

        return results.Select(x => new SearchToolResponseItem
        {
            ArxivId = x.ArxivId,
            Distance = x.Distance,
            Title = x.Title,
            Summary = x.Summary,
            Comments = x.Comments,
            Metadata = x.Metadata,
            PdfUri = x.PdfUri,
            PublishedDate = x.Published?.ToString("o")
        }).ToList();
    }
}
