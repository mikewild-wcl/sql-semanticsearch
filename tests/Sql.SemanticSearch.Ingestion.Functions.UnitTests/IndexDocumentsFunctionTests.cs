using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Sql.SemanticSearch.Core.Requests;
using Sql.SemanticSearch.Ingestion.Functions.UnitTests.Builders;
using System.Text.Json;

namespace Sql.SemanticSearch.Ingestion.Functions.UnitTests;

public class IndexDocumentsFunctionTests
{
    private readonly IndexDocumentsFunction _sut = new(NullLogger<IndexDocumentsFunction>.Instance);

    [Fact]
    public void Run_ReturnsOk_ForValidIds()
    {
        var requestObj = new IndexingRequest { Ids = ["id1", "id2"] };
        var body = JsonSerializer.Serialize(requestObj);
        var httpRequest = HttpRequestBuilder.Build("POST", "/api/index-documents", body: body, contentType: "application/json");
        var result = _sut.Run(httpRequest, requestObj);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Indexing request successfully processed 2 documents.", okResult.Value);
    }

    [Fact]
    public void Run_ReturnsBadRequest_ForEmptyIds()
    {
        var requestObj = new IndexingRequest { Ids = [] };
        var body = JsonSerializer.Serialize(requestObj);
        var httpRequest = HttpRequestBuilder.Build("POST", "/api/index-documents", body: body, contentType: "application/json");
        var result = _sut.Run(httpRequest, requestObj);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No ids provided.", badRequest.Value);
    }

    [Fact]
    public void Run_ReturnsBadRequest_ForNullIds()
    {
        var requestObj = new IndexingRequest { Ids = null! };
        var body = JsonSerializer.Serialize(requestObj);
        var httpRequest = HttpRequestBuilder.Build("POST", "/api/index-documents", body: body, contentType: "application/json");
        var result = _sut.Run(httpRequest, requestObj);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No ids provided.", badRequest.Value);
    }
}
