using Sql.SemanticSearch.Core.Requests;
using System.Text.Json;

namespace Sql.SemanticSearch.Core.UnitTests.Requests;

public class IndexingRequestTests
{
    [Fact]
    public void IndexingRequest_CanBeCreated_WithIds()
    {
        // Arrange
        var ids = new List<string> { "1234.5678", "9876.5432" };

        // Act
        var request = new IndexingRequest
        {
            Ids = ids
        };

        // Assert

        request.ShouldNotBeNull();
        request.Ids.ShouldBeEquivalentTo(ids);
    }

    [Fact]
    public void IndexingRequest_SerializesToJson_WithIdsProperty()
    {
        // Arrange
        var ids = new[] { "a", "b" };
        var request = new IndexingRequest { Ids = ids };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.ShouldContain("\"ids\":");
        json.ShouldContain("a");
        json.ShouldContain("b");
        json.ShouldNotContain("Ids", Case.Sensitive);
    }

    [Fact]
    public void IndexingRequest_DeserializesFromJson_WithIdsProperty()
    {
        // Arrange
        var json =
            """
            {            
              "ids": [
                "1234.5678",
                "1000.00001"
              ]
            }
            """;

        // Act
        var request = JsonSerializer.Deserialize<IndexingRequest>(json);

        // Assert
        request.ShouldNotBeNull();
        request.Ids.Count.ShouldBe(2);
        request.Ids.ShouldContain("1234.5678");
        request.Ids.ShouldContain("1000.00001");
        
        /* Fails because deserialization does not create ReadOnlyCollection */
        // request.Ids.ShouldBeEquivalentTo(new IndexingRequest { Ids = [ "1234.5678", "1000.00001" ] });
    }
}

