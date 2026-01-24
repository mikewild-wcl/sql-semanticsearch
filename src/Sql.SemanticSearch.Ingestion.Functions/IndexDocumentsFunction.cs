using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Messages;
using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.Ingestion.Functions;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to catch general exceptions for top-level function failures")]
public class IndexDocumentsFunction(
    IIngestionService ingestionService,
    ILogger<IndexDocumentsFunction> logger)
{
    private readonly IIngestionService _ingestionService = ingestionService;
    private readonly ILogger<IndexDocumentsFunction> _logger = logger;

    private static readonly Action<ILogger, int, Exception?> _logFunctionTriggered =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(0, nameof(IndexDocumentsFunction)),
            "Indexing function http trigger called with {Count} document ids.");

    private static readonly Action<ILogger, Exception?> _logNullOrEmptyIndexingRequestWarning =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(0, nameof(IndexDocumentsFunction)),
            "Indexing function called with no document ids.");

    private static readonly Action<ILogger, Exception?> _logFunctionFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(0, nameof(IndexDocumentsFunction)),
            "An error occurred while processing the indexing request.");

    [Function("IndexArxivDocuments")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "index-documents")]
        HttpRequest _,
        [Microsoft.Azure.Functions.Worker.Http.FromBody]
        IndexingRequest indexingRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            if (indexingRequest?.Ids is null || indexingRequest.Ids.Count == 0)
            {
                _logNullOrEmptyIndexingRequestWarning(_logger, null);
                return new BadRequestObjectResult("No ids provided.");
            }

            _logFunctionTriggered(_logger, indexingRequest.Ids.Count, null);

            await _ingestionService.ProcessIndexingRequest(indexingRequest, cancellationToken);

            return new OkObjectResult($"Indexing request successfully processed {indexingRequest.Ids.Count} documents.");
        }
        catch (Exception ex)
        {
            _logFunctionFailed(_logger, ex);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}