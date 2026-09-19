using Sql.SemanticSearch.Core.Extensions;
using Sql.SemanticSearch.Shared;

namespace Sql.SemanticSearch.Core.UnitTests.Extensions;

public class McpHelpersTests
{
    [Fact]
    public void GetMarkItDownMcpServerUrl_ShouldReturnPortInUri_WhenEnvironmentVariableIsSet() 
    {
        // Arrange
#pragma warning disable CA5394 // Do not use insecure randomness
        var random = new Random();
        var port = random.Next(1000, 9999);
#pragma warning restore CA5394 // Do not use insecure randomness

        Environment.SetEnvironmentVariable(EnvironmentVariableNames.MarkitdownMcpUri, $"http://localhost:{port}");

        // Act
        var result = McpHelpers.GetMarkItDownMcpServerUrl();

        // Assert
        Assert.Equal(new Uri($"http://localhost:{port}/mcp"), result);
        result.ShouldBe(new Uri($"http://localhost:{port}/mcp")); // Using Shouldly for assertion   
    }

    [Fact]
    public void GetMarkItDownMcpServerUrl_ShouldReturnDefaultUri_WhenEnvironmentVariableIsNotSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariableNames.MarkitdownMcpUri, null);

        // Act
        var result = McpHelpers.GetMarkItDownMcpServerUrl();

        // Assert
        Assert.Equal(new Uri("http://localhost:3001/mcp"), result);
        result.ShouldBe(new Uri("http://localhost:3001/mcp")); // Using Shouldly for assertion   
    }
}
