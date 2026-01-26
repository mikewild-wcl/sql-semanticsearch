using Sql.SemanticSearch.Core.Data.TypeHandlers;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Serialization;
using System.Data;
using System.Text.Json;

namespace Sql.SemanticSearch.Core.UnitTests.Data.TypeHandles;

public class MetadataTypeHandlerTests
{
    [Fact]
    public void SetValue_WithNullMetadata_SetsDbTypeStringAndDBNullValue()
    {
        // Arrange
        var sut = new MetadataTypeHandler();
        var parameter = Substitute.For<IDbDataParameter>();

        // Act
        sut.SetValue(parameter, null);

        // Assert
        parameter.Received(1).DbType = DbType.String;
        parameter.Received(1).Value = DBNull.Value;
    }

    [Fact]
    public void SetValue_WithMetadata_SetsDbTypeStringAndSerializedJsonValue()
    {
        // Arrange
        var sut = new MetadataTypeHandler();
        var parameter = Substitute.For<IDbDataParameter>();

        var metadata = new DocumentMetadata
        {
            Authors = ["Alice", "Bob"],
            Categories = ["cs.CL", "stat.ML"]
        };

        var expectedJson = JsonSerializer.Serialize(metadata, SerializerOptions.CamelCaseSerialierOptions);

        // Act
        sut.SetValue(parameter, metadata);

        // Assert
        parameter.Received(1).DbType = DbType.String;
        parameter.Received(1).Value = expectedJson;
    }

    [Fact]
    public void Parse_WithValidJson_ReturnsMetadata()
    {
        // Arrange
        var sut = new MetadataTypeHandler();

        var json =
            """
            {
              "authors": ["Alice", "Bob"],
              "categories": ["cs.CL", "stat.ML"]
            }
            """;

        var j1 = JsonSerializer.Deserialize<DocumentMetadata>(json, SerializerOptions.CamelCaseSerialierOptions);

        json = """{"authors":["Alice","Bob"],"categories":["cs.CL","stat.ML"]}""";
        var j2 = JsonSerializer.Deserialize<DocumentMetadata>(json, SerializerOptions.CamelCaseSerialierOptions);

        json = """{"authors":["Alice","Bob"],"categories":["cs.CL","stat.ML"]}""";
        var j3 = JsonSerializer.Deserialize<DocumentMetadata>(json, SerializerOptions.CamelCaseSerialierOptions);

        // Act
        var result = sut.Parse(json);

        // Assert
        result.ShouldNotBeNull();
        result.Authors.ShouldBe(["Alice", "Bob"]);
        result.Categories.ShouldBe(["cs.CL", "stat.ML"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithEmptyOrWhitespace_ReturnsDefaultMetadata(string input)
    {
        // Arrange
        var sut = new MetadataTypeHandler();

        // Act
        var result = sut.Parse(input);

        // Assert
        result.ShouldNotBeNull();
        result.Authors.ShouldBeEmpty();
        result.Categories.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_WithDBNull_ReturnsDefaultMetadata()
    {
        // Arrange
        var sut = new MetadataTypeHandler();

        // Act
        var result = sut.Parse(DBNull.Value);

        // Assert
        result.ShouldNotBeNull();
        result.Authors.ShouldBeEmpty();
        result.Categories.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_WithNullObject_ReturnsDefaultMetadata()
    {
        // Arrange
        var sut = new MetadataTypeHandler();

        // Act
        var result = sut.Parse(null!);

        // Assert
        result.ShouldNotBeNull();
        result.Authors.ShouldBeEmpty();
        result.Categories.ShouldBeEmpty();
    }
}