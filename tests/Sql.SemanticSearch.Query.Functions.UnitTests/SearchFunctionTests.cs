using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search.Interfaces;
using Sql.SemanticSearch.Tests.Helpers.Builders;
using System.Text.Json;

namespace Sql.SemanticSearch.Query.Functions.UnitTests;

public class SearchFunctionTests
{
    private readonly ISearchService _searchServiceSubstitute;
    private readonly SearchFunction _sut;

    public SearchFunctionTests()
    {
        _searchServiceSubstitute = Substitute.For<ISearchService>();

        _sut = new(_searchServiceSubstitute, NullLogger<SearchFunction>.Instance);
    }

    [Fact]
    public async Task Run_ReturnsOk_ForValidSearchs()
    {
        // Arrange
        var requestObj = new SearchRequest { Query = "I want to know evertyhing" };
        var body = JsonSerializer.Serialize(requestObj);
        var httpRequest = HttpRequestBuilder.Build("POST", "/api/search", body: body, contentType: "application/json");

        // Act
        var result = await _sut.Run(httpRequest, requestObj, TestContext.Current.CancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe("Sample search result.");
    }

}
