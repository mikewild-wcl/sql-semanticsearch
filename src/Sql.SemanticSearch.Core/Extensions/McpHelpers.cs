using Sql.SemanticSearch.Shared;

namespace Sql.SemanticSearch.Core.Extensions;

public static  class McpHelpers
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private const string DefaultMarkitdownMcpUri = "http://localhost:3001";
#pragma warning restore S1075 // URIs should not be hardcoded

    public static Uri GetMarkItDownMcpServerUrl()
    {
        var mcpUri = Environment.GetEnvironmentVariable(EnvironmentVariableNames.MarkitdownMcpUri);
        if (string.IsNullOrEmpty(mcpUri))
        {
            mcpUri = DefaultMarkitdownMcpUri;
        }

        return new Uri($"{mcpUri.TrimEnd('/')}/mcp");
    }
}
