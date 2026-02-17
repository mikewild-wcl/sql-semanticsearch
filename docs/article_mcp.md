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
 
### Source code
[!NOTE] 
> Source code is available on [GitHub](https://github.com/mikewild-wcl/sql-semanticsearch)


## References

- [Azure API Management Your Auth Gateway For MCP Servers](https://techcommunity.microsoft.com/blog/integrationsonazureblog/azure-api-management-your-auth-gateway-for-mcp-servers/4402690?utm_id=Luca+Congiu&utm_source=linkedin)
