using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.Requests;

namespace Sql.SemanticSearch.Ingestion.Functions;

public class IndexDocumentsFunction(ILogger<IndexDocumentsFunction> logger)
{
    private readonly ILogger<IndexDocumentsFunction> _logger = logger;

    private static readonly Action<ILogger, int, Exception?> _logFunctionTriggered =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(0, nameof(IndexDocumentsFunction)),
            "IngestFromUriFunction http function triggered with {Count} document ids.");

    private static readonly Action<ILogger, Exception?> _logNullOrEmptyIndexingRequestWarning =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(0, nameof(IndexDocumentsFunction)),
            "IngestFromUriFunction called with no document ids.");

    private static readonly Action<ILogger, string, Exception?> _logDocumentIdProcessStarted =
    LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(0, nameof(IndexDocumentsFunction)),
        "IngestFromUriFunction processing document id: {Id}.");    

    [Function("IndexArxivDocuments")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "index-documents")]
        HttpRequest _,
        [Microsoft.Azure.Functions.Worker.Http.FromBody]
        IndexingRequest indexingRequest)
    {
        if (indexingRequest?.Ids is null || indexingRequest.Ids.Count == 0)
        {
            _logNullOrEmptyIndexingRequestWarning(_logger, null);
            return new BadRequestObjectResult("No ids provided.");
        }

        _logFunctionTriggered(_logger, indexingRequest.Ids.Count, null);

        foreach (var id in indexingRequest.Ids)
        {
            _logDocumentIdProcessStarted(_logger, id, null);
        }

        return new OkObjectResult($"Indexing request successfully processed {indexingRequest.Ids.Count} documents.");
    }
}