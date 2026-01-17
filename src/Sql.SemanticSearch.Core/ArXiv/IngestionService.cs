using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Requests;
using System.Text.Json;

namespace Sql.SemanticSearch.Core.ArXiv;

public class IngestionService(
    IArxivApiClient arxivApiClient,
    IDatabaseConnection databaseConnection,
    ILogger<IngestionService> logger) : IIngestionService
{
    private readonly IArxivApiClient _arxivApiClient = arxivApiClient;
    private readonly IDatabaseConnection _databaseConnection = databaseConnection;
    private readonly ILogger<IngestionService> _logger = logger;

    private static JsonSerializerOptions CamelCaseSerialierOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Action<ILogger, string, Exception?> _logDocumentIdProcessStarted =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(0, nameof(IngestionService)),
            "Processing document id: {Id}.");

    private static readonly Action<ILogger, string, Exception?> _logErrorStoringPaper =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(IngestionService)),
            "Error storing paper with id {Id} in database.");

    public async Task ProcessIndexingRequest(IndexingRequest indexingRequest)
    {
        ArgumentNullException.ThrowIfNull(indexingRequest);

        foreach (var id in indexingRequest.Ids)
        {
            _logDocumentIdProcessStarted(_logger, id, null);
            var paper = await _arxivApiClient.GetPaperInfo(id);

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                //await StorePaperInDatabase(paper);
                var metadata = new
                {
                    paper.Authors,
                    paper.Categories
                };
                var metadataString = JsonSerializer.Serialize(metadata, CamelCaseSerialierOptions);

                var dto = new
                {
                    ArxivId = paper.Id,
                    paper.Title,
                    paper.Summary,
                    paper.Comments,
                    Metadata = metadataString,
                    PdfUri = paper.PdfUri?.ToString(),
                    paper.Published
                };

                // TODO: Use ExecuteScalar and add get the id out?
                var documentId = await _databaseConnection.ExecuteScalarAsync<int>(
                    """
                    INSERT INTO dbo.Documents ([ArxivId], [Title], [Summary], [Comments], [Metadata], [PdfUri], [Published])
                    VALUES (@ArxivId, @Title, @Summary, @Comments, @Metadata, @PdfUri, @Published);
                    SELECT CAST(SCOPE_IDENTITY() as int);
                    """, dto);
                /*
        [Id] INT IDENTITY CONSTRAINT PK_Documents primary key,
        [ArxivId] NVARCHAR(50) NULL,
        [Title] nvarchar(300) NOT NULL,
        [Summary] nvarchar(max) NULL,
        [Metadata] JSON NULL,
        [Url] NVARCHAR(1000) NOT NULL,
        [Published] DATETIME2(0) NOT NULL,
        [Updated] DATETIME2(7) NULL,
        [CreatedOn] DATETIME2(7) NOT NULL CONSTRAINT DF_Documents_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        [LastUpdatedOn] datetime2(0) NULL
                 */
            }
            catch (Exception ex)
            {
                _logErrorStoringPaper(_logger, id, ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

        }
    }
}
