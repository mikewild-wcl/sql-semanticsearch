using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.Query.Functions;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to catch general exceptions for top-level function failures")]
public class SearchFunction(
    ISearchService searchService,
    ILogger<SearchFunction> logger)
{
    private readonly ISearchService _searchService = searchService;
    private readonly ILogger<SearchFunction> _logger = logger;

    private static readonly Action<ILogger, Exception?> _logFunctionTriggered =
    LoggerMessage.Define(
        LogLevel.Information,
        new EventId(0, nameof(SearchFunction)),
        "Search function http trigger called.");

    private static readonly Action<ILogger, Exception?> _logFunctionFailed =
    LoggerMessage.Define(
        LogLevel.Error,
        new EventId(0, nameof(SearchFunction)),
        "An error occurred while processing the indexing request.");

    [Function("Search")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "search")]
        HttpRequest _,
        [Microsoft.Azure.Functions.Worker.Http.FromBody]
        SearchRequest searchRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            _logFunctionTriggered(_logger, null);

            var results = await _searchService.Search(searchRequest, cancellationToken);

            return new OkObjectResult("Sample search result.");
        }
        catch (Exception ex)
        {
            _logFunctionFailed(_logger, ex);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}