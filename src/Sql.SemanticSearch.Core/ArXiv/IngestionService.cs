using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Requests;

namespace Sql.SemanticSearch.Core.ArXiv;

public class IngestionService(
    IArxivApiClient arxivApiClient,
    ILogger<IngestionService> logger) : IIngestionService
{
    private readonly IArxivApiClient _arxivApiClient = arxivApiClient;
    private readonly ILogger<IngestionService> _logger = logger;

    private static readonly Action<ILogger, string, Exception?> _logDocumentIdProcessStarted =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(0, nameof(IngestionService)),
            "Processing document id: {Id}.");

    public async Task ProcessIndexingRequest(IndexingRequest indexingRequest)
    {
        ArgumentNullException.ThrowIfNull(indexingRequest);

        foreach (var id in indexingRequest.Ids)
        {
            _logDocumentIdProcessStarted(_logger, id, null);
            await _arxivApiClient.GetPaperInfo(id);
        }
    }
}
