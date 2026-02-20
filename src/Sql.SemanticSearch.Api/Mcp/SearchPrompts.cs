using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Sql.SemanticSearch.Api.Mcp;

[McpServerPromptType]
public sealed class SearchPrompts
{
    [McpServerPrompt(Name = McpConstants.SearchArxivPrompt)]
    [Description("Search for arXiv academic papers on a given topic using semantic search.")]
    public string SearchArxivPapers(
        [Description("The topic or subject to search for in arXiv papers.")] string topic)
    {   
        return $"Use the {McpConstants.SemanticSearchTool} MCP tool to search for arXiv papers on the following topic: {topic}";
    }
}
