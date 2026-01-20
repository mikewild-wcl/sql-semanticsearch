using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
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

        await foreach (var paper in _arxivApiClient.GetPaperInfo(indexingRequest.Ids))
        {
            try
            {
                //_logDocumentIdProcessStarted(_logger, id, null);
                //var paper = await _arxivApiClient.GetPaperInfo(id);
                _logDocumentIdProcessStarted(_logger, paper.Id, null);

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
                }
                catch (Exception ex)
                {
                    _logErrorStoringPaper(_logger, paper?.Id, ex);
                }
#pragma warning restore CA1031 // Do not catch general exception types            

            }
            catch (HttpRequestException ex)
            {
                //_logErrorStoringPaper(_logger, id, ex);
                throw new ArxivApiException($"Error fetching paper info: {ex.Message}", ex);
            }
        }
    }
}
