using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.ArXiv.Exceptions;
using Sql.SemanticSearch.Core.ArXiv.Interfaces;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Requests;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Sql.SemanticSearch.Core.ArXiv;

public class IngestionService(
    IArxivApiClient arxivApiClient,
    IDatabaseConnection databaseConnection,
    AISettings aiSettings,
    ILogger<IngestionService> logger) : IIngestionService
{
    private readonly IArxivApiClient _arxivApiClient = arxivApiClient;
    private readonly IDatabaseConnection _databaseConnection = databaseConnection;
    private readonly AISettings _aiSettings = aiSettings;
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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Using general exception types during development.")]
    public async Task ProcessIndexingRequest(IndexingRequest indexingRequest, CancellationToken? cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(indexingRequest);

        //TODO: Use new method and pass in cancellation token
        await foreach (var paper in _arxivApiClient.GetPaperInfo(indexingRequest.Ids))
        {
            try
            {
                //_logDocumentIdProcessStarted(_logger, id, null);
                //var paper = await _arxivApiClient.GetPaperInfo(id);
                _logDocumentIdProcessStarted(_logger, paper.Id, null);

                try
                {
                    //await StorePaperInDatabase(paper);
                    var metadata = new
                    {
                        paper.Authors,
                        paper.Categories
                    };
                    var metadataString = JsonSerializer.Serialize(metadata, CamelCaseSerialierOptions);

                    var document = new
                    {
                        ArxivId = paper.Id,
                        paper.Title,
                        paper.Summary,
                        paper.Comments,
                        Metadata = metadataString,
                        PdfUri = paper.PdfUri?.ToString(),
                        paper.Published
                    };

                    //TODO: Wrap in resliience pipeline
                    await _databaseConnection.OpenConnection();
                    try
                    {
                        using var transaction = _databaseConnection.BeginTransaction();

                        var documentId = await _databaseConnection.ExecuteScalarAsync<int>(
                            """
                            INSERT INTO dbo.Documents ([ArxivId], [Title], [Summary], [Comments], [Metadata], [PdfUri], [Published])                            
                            VALUES (@ArxivId, @Title, @Summary, @Comments, @Metadata, @PdfUri, @Published);
                            SELECT CAST(SCOPE_IDENTITY() as int);
                            """,
                            document,
                            transaction: transaction);

                        var summaryEmbedding = new
                        {
                            Id = documentId,
                            paper.Title,
                            paper.Summary,
                            paper.Comments,
                            Metadata = metadataString,
                            PdfUri = paper.PdfUri?.ToString(),
                            paper.Published
                        };

                        await _databaseConnection.ExecuteAsync(
                            $"""
                            INSERT INTO dbo.DocumentSummaryEmbeddings ([Id], [Embedding])
                            SELECT @Id,
                                   AI_GENERATE_EMBEDDINGS(d.[Summary] USE MODEL {_aiSettings.ExternalEmbeddingModel})
                            FROM dbo.Documents d
                            WHERE d.[Id] = @Id
                              AND d.[Summary] IS NOT NULL;
                            """,
                            new { Id = documentId },
                            transaction: transaction);

                        await _databaseConnection.ExecuteAsync(
                            $"""
                            INSERT INTO dbo.DocumentMetadataEmbeddings ([Id], [Embedding])
                            SELECT @Id,
                                   AI_GENERATE_EMBEDDINGS(CAST(d.[Metadata] AS NVARCHAR(MAX)) USE MODEL {_aiSettings.ExternalEmbeddingModel})
                            FROM dbo.Documents d
                            WHERE d.[Id] = @Id
                              AND d.[Metadata] IS NOT NULL;
                            """,
                            new { Id = documentId },
                            transaction: transaction);

                        transaction.Commit();
                    }
                    finally
                    {
                        await _databaseConnection.CloseConnection();
                    }
                }
                catch (Exception ex)
                {
                    _logErrorStoringPaper(_logger, paper?.Id, ex);
                }
            }
            catch (HttpRequestException ex)
            {
                //_logErrorStoringPaper(_logger, id, ex);
                throw new ArxivApiException($"Error fetching paper info: {ex.Message}", ex);
            }
        }
    }
}
