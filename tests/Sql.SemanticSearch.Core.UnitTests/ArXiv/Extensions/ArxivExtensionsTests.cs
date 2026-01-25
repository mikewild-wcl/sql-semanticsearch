using Sql.SemanticSearch.Core.ArXiv.Extensions;

namespace Sql.SemanticSearch.Core.UnitTests.ArXiv.Extensions;

public class ArxivExtensionsTests
{
    [Theory]
    [InlineData("2101.12345", true)]
    [InlineData("0704.0001v2", true)]
    [InlineData("hep-th/9901001", true)]
    [InlineData("math.GT/0309136v3", true)]
    [InlineData("21.12345", false)]
    [InlineData("2101.123", false)]
    [InlineData("hep-th9901001", false)]
    [InlineData("2101.12345v", false)]
    [InlineData("xxxx.yyyy", false)]
    public void IsValid_VariousInputs_ReturnsExpectedResult(string input, bool expected)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Act
        var result = input.IsValidId();

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("https://arxiv.org/abs/2107.05580v1", "2107.05580v1")]
    [InlineData("https://arxiv.org/abs/quant-ph/0201082v1", "quant-ph/0201082v1")]
    [InlineData("https://arxiv.org/abs/1409.0473v7", "1409.0473v7")]
    [InlineData("some-other-url", "some-other-url")]
    [InlineData("", "")]
    public void ToShortId_VariousInputs_ReturnsExpectedShortId(string input, string expected)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Act
        var result = input.ToShortId();

        // Assert
        result.ShouldBe(expected);
    }
}