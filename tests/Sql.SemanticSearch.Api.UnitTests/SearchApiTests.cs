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
            .Returns(
            [
                new() { ArxivId = "1001.1234", Title = "Test 1", Distance = 0.2015f },
                new() { ArxivId = "2002.1234", Title = "Test 2", Distance = 0.3031f }
            ]);

        using var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/search",
            new SearchRequest { Query = "test query" },
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<SearchResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        results.ShouldNotBeNull();
        results.Items.ShouldNotBeEmpty();

        results.Items.ShouldContain(r => 
            r.ArxivId == "1001.1234" && 
            r.Title == "Test 1" && 
            Math.Abs(r.Distance - 0.2015f) < 0.0001f);

        results.Items.ShouldContain(r =>
            r.ArxivId == "2002.1234" &&
            r.Title == "Test 2" &&
            Math.Abs(r.Distance - 0.3031f) < 0.0001f);

        await _fixture.SearchService.Received(1).Search(
            Arg.Is<SearchRequest>(r => r.Query == "test query"),
            Arg.Any<CancellationToken>());
    }
}
