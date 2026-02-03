using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Tests.Helpers.Builders;
using System.Text.Json;

namespace Sql.SemanticSearch.Ingestion.Functions.UnitTests;

public class IndexDocumentsFunctionTests
{
    private readonly IIngestionService _ingestionServiceSubstitute;
    private readonly IndexDocumentsFunction _sut;

    public IndexDocumentsFunctionTests()
    {
        _ingestionServiceSubstitute = Substitute.For<IIngestionService>();

        _sut = new(_ingestionServiceSubstitute, NullLogger<IndexDocumentsFunction>.Instance);
    }

    [Fact]
    public async Task Run_ReturnsOk_ForValidIds()
    {
        // Arrange
        var expected = new
        {
            status = 200, //HttpStatusCode.OK,
            result = "Indexing request successfully processed 2 documents."
        };

        var requestObj = new IndexingRequest { Ids = ["id1", "id2"] };
        var body = JsonSerializer.Serialize(requestObj);
        var httpRequest = HttpRequestBuilder.Build(
            "POST", 
            "/api/index-documents", 
            body: body, 
            contentType: "application/json");

        // Act
        var result = await _sut.Run(httpRequest, requestObj, TestContext.Current.CancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldNotBeNull();
                
        /* Compare serialized values, to avoid type mismatches or the need for reflection */
        JsonSerializer.Serialize(okResult.Value)
            .ShouldBe(JsonSerializer.Serialize(expected));
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_ForEmptyIds()
    {
        // Arrange
        var requestObj = new IndexingRequest { Ids = [] };
        var body = JsonSerializer.Serialize(requestObj);
        var httpRequest = HttpRequestBuilder.Build(
            "POST", 
            "/api/index-documents", 
            body: body, 
            contentType: "application/json");

        // Act
        var result = await _sut.Run(httpRequest, requestObj, TestContext.Current.CancellationToken);

        // Assert
        var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("No ids provided.");
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_ForNullIds()
    {
        // Arrange
        var requestObj = new IndexingRequest { Ids = null! };
        var body = JsonSerializer.Serialize(requestObj);
        var httpRequest = HttpRequestBuilder.Build(
            "POST", 
            "/api/index-documents",
            body: body, 
            contentType: "application/json");

        // Act
        var result = await _sut.Run(httpRequest, requestObj, TestContext.Current.CancellationToken);

        // Assert
        var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("No ids provided.");
    }
}
