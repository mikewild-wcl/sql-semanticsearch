using Shouldly;
using Xunit;

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
        var result = input.ToShortId();

        // Assert
        result.ShouldBe(expected);
    }
}