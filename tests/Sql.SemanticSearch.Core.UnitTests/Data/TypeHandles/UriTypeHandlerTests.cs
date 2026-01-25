using Sql.SemanticSearch.Core.Data.TypeMappers;
using System.Data;

namespace Sql.SemanticSearch.Core.UnitTests.Data.TypeHandles;

public class UriTypeHandlerTests
{
    [Fact]
    public void SetValue_WithNullUri_SetsDbTypeStringAndDBNullValue()
    {
        // Arrange
        var sut = new UriTypeHandler();
        var parameter = Substitute.For<IDbDataParameter>();

        // Act
        sut.SetValue(parameter, null);

        // Assert
        parameter.Received(1).DbType = DbType.String;
        parameter.Received(1).Value = DBNull.Value;
    }

    [Fact]
    public void SetValue_WithUri_SetsDbTypeStringAndStringValue()
    {
        // Arrange
        var sut = new UriTypeHandler();
        var parameter = Substitute.For<IDbDataParameter>();
        var uri = new Uri("https://example.com/path?q=1");

        // Act
        sut.SetValue(parameter, uri);

        // Assert
        parameter.Received(1).DbType = DbType.String;
        parameter.Received(1).Value = uri.ToString();
    }

    [Theory]
    [InlineData("https://example.com/a/b?c=1", "https://example.com/a/b?c=1")]
    [InlineData("relative/path", "relative/path")]
    public void Parse_WithValidUriString_ReturnsUri(string input, string expected)
    {
        // Arrange
        var sut = new UriTypeHandler();

        // Act
        var result = sut.Parse(input);

        // Assert
        result.ShouldNotBeNull();
        result.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithWhitespaceUriString_ReturnsNull(string input)
    {
        // Arrange
        var sut = new UriTypeHandler();

        // Act
        var result = sut.Parse(input);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Parse_WithNullObject_ReturnsNull()
    {
        // Arrange
        var sut = new UriTypeHandler();

        // Act
        var result = sut.Parse(null!);

        // Assert
        result.ShouldBeNull();
    }
}