using Sql.SemanticSearch.Core.Messages;
using System.Net;
using System.Net.Http.Json;
using Sql.SemanticSearch.Api.UnitTests.Fixtures;

namespace Sql.SemanticSearch.Api.UnitTests;

public class SearchApiTests(SearchApiFixture fixture) : IClassFixture<SearchApiFixture>
{
    private readonly SearchApiFixture _fixture = fixture;

    [Fact]
    public async Task Search_Post_WithValidRequest_ReturnsOk()
    {
        // Arrange
        _fixture.SearchService
            .Search(Arg.Any<SearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(["result-1", "result-2"]);

        using var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/search",
            new SearchRequest { Query = "test query" },
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<string[]>(
            cancellationToken: TestContext.Current.CancellationToken);

        payload.ShouldNotBeNull();
        payload.ShouldBe(["result-1", "result-2"]);

        await _fixture.SearchService.Received(1).Search(
            Arg.Is<SearchRequest>(r => r.Query == "test query"),
            Arg.Any<CancellationToken>());
    }
}
