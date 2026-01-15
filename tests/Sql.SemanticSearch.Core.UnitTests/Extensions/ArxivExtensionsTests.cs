using Sql.SemanticSearch.Core.ArXiv.Extensions; 

namespace Sql.SemanticSearch.Core.ArXiv.UnitTests.Extensions;

public class ArxivExtensionsTests
{
    [Theory]
    [InlineData("https://arxiv.org/abs/2107.05580v1", "2107.05580v1")]
    [InlineData("https://arxiv.org/abs/quant-ph/0201082v1", "quant-ph/0201082v1")]
    [InlineData("https://arxiv.org/abs/1409.0473v7", "1409.0473v7")]
    [InlineData("some-other-url", "some-other-url")]
    [InlineData("", "")]
    public void ToShortId_VariousInputs_ReturnsExpectedShortId(string input, string expected)
    {
        // Act
#pragma warning disable CA1062 // Validate arguments of public methods
        //TODO: Decide if this needs to be fixed
        var result = input.ToShortId();
#pragma warning restore CA1062 // Validate arguments of public methods

        // Assert
        result.ShouldBe(expected);
    }
}