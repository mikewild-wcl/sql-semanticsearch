using Sql.SemanticSearch.Api.Mcp;

namespace Sql.SemanticSearch.Api.UnitTests.Mcp;

public class SearchPromptsTests
{
    [Fact]
    public void SearchArxivPapers_ReturnsExpectedPromptString()
    {
        // Arrange
        var prompts = new SearchPrompts();
        var topic = "quantum computing";

        // Act
        var result = prompts.SearchArxivPapers(topic);

        // Assert
        result.ShouldBe($"Use the {McpConstants.SemanticSearchTool} MCP tool to search for arXiv papers on the following topic: {topic}");
    }

    [Fact]
    public void SearchArxivPapers_WithEmptyTopic_ReturnsPromptWithEmptyTopic()
    {
        // Arrange
        var prompts = new SearchPrompts();
        var topic = string.Empty;

        // Act
        var result = prompts.SearchArxivPapers(topic);

        // Assert
        result.ShouldBe($"Use the {McpConstants.SemanticSearchTool} MCP tool to search for arXiv papers on the following topic: ");
    }
}
