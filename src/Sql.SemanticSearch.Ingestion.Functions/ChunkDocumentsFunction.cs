using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.Chunking;
using Sql.SemanticSearch.Core.Chunking.Interfaces;
using Sql.SemanticSearch.Shared;

namespace Sql.SemanticSearch.Ingestion.Functions;

public class ChunkDocumentsFunction(
    IDocumentChunkingService chunkingService,
    ILogger<ChunkDocumentsFunction> logger)
{
    private readonly IDocumentChunkingService _chunkingService = chunkingService;
    private readonly ILogger<ChunkDocumentsFunction> _logger = logger;

    private static readonly Action<ILogger, SqlChangeOperation, int, string, string?, Exception?> _logSqlChangeReceived =
        LoggerMessage.Define<SqlChangeOperation, int, string, string?>(
        LogLevel.Information,
        new EventId(0, nameof(ChunkDocumentsFunction)),
        "SQL change: {Operation}, Id: {Id}, Title: {Title}, Url: {PdfUri}");

    [Function("ChunkDocumentsFunction")]
    public async Task Run(
        [SqlTrigger("[dbo].[Documents]", ResourceNames.SqlDatabase)]
        IReadOnlyList<SqlChange<DatabaseDocument>> changes,
        //FunctionContext context,
        CancellationToken cancellationToken)
    {
        if (changes is null) return;

        foreach (SqlChange<DatabaseDocument> change in changes)
        {
            var document = change.Item;
            _logSqlChangeReceived(_logger, change.Operation, document.Id, document.Title, document.PdfUri, null);

            await _chunkingService.IndexDocument(document);
        }
    }
}
