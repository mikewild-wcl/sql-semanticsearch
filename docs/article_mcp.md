# Adding Model Context Protocol for SQL Server 2025 Semantic Search 
## Exposing search using MCP

Intro - describe MCP and how it can be used to expose the semantic search capabilities of SQL Server 2025. Talk about the benefits of using MCP for this, such as simplified API management, security, and scalability.


## Implementation

Added nuget package `Microsoft.AspNetCore.ModelContextProtocol` to the API project and implemented MCP endpoint. The endpoint accepts search queries, forwards them to the database, and returns results in a structured format.

The endpoint uses the new `SearchTool` class in SQL Server 2025 to perform semantic search. The API translates the incoming search query into a format that can be understood by the database, executes the search, and then formats the results for the client.

How it works
- The MCP server is hosted at /mcp using HTTP+SSE transport (Streamable HTTP).
- Any MCP-compatible client (e.g., VS Code Copilot, Claude Desktop) can connect to https://<your-api-host>/mcp.
- The semantic_search tool calls your existing ISearchService.Search() — no new database or AI logic needed.
- The tool is automatically discovered via WithToolsFromAssemblyContaining<SearchTool>().

- Example MCP client config (e.g., mcp.json for VS Code):
```
{
  "servers": {
    "sql-semantic-search": {
      "url": "https://localhost:7253/mcp"
    }
  }
}
```

I have added `SearchTools` to the API project, which implements the `ITool` interface from the MCP library. This tool is responsible for handling search requests from MCP clients, executing the search logic using the existing `ISearchService`, and returning results in a format that MCP clients can understand.

The tool returns results in a structured format that includes the document title, a snippet of the content, and a relevance score. This allows MCP clients to display search results in a user-friendly way.

I have also added `SearchPrompts` which contains the prompt templates for the `SearchTool`. These prompts are used to generate the appropriate queries for the database based on the user's search input.

 
### Source code
[!NOTE] 
> Source code is available on [GitHub](https://github.com/mikewild-wcl/sql-semanticsearch)


## References

- [Azure API Management Your Auth Gateway For MCP Servers](https://techcommunity.microsoft.com/blog/integrationsonazureblog/azure-api-management-your-auth-gateway-for-mcp-servers/4402690?utm_id=Luca+Congiu&utm_source=linkedin)
