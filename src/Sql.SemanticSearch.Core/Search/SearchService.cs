using Microsoft.Extensions.Logging;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Core.Messages;
using Sql.SemanticSearch.Core.Search.Interfaces;

namespace Sql.SemanticSearch.Core.Search;

public class SearchService(
    IDatabaseConnection databaseConnection,
    AISettings aiSettings,
    ILogger<SearchService> logger) : ISearchService
{
    private readonly IDatabaseConnection _databaseConnection = databaseConnection;
    private readonly AISettings _aiSettings = aiSettings;
    private readonly ILogger<SearchService> _logger = logger;

    private static readonly Action<ILogger, string, int, Exception?> _logSearch =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(0, nameof(SearchService)),
            "Processing search query {Query}. Top-k is {K}");

    private static readonly Action<ILogger, int, Exception?> _logSearchResult =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(0, nameof(SearchService)),
            "Search returned {Count} results.");

    private static readonly Action<ILogger, Exception?> _logQueryError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(SearchService)),
            "Error querying database.");

    public async Task<IEnumerable<SearchResultItem>> Search(SearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(searchRequest?.Query))
        {
            return [];
        }

        _logSearch(_logger, searchRequest.Query, searchRequest.Top, null);

        try
        {
            var results = (await _databaseConnection.QueryAsync<SearchResultItem>(
                $"""
                DECLARE @vector VECTOR({_aiSettings.EmbeddingModelDimensions});
                
                SELECT @vector = AI_GENERATE_EMBEDDINGS(@query USE MODEL {_aiSettings.ExternalEmbeddingModel});
                
                SELECT TOP(@k) [ArxivId],
                               [Title],
                               [Summary],
                               [Comments],
                               [Metadata],
                               [PdfUri],
                               [Published],
                               VECTOR_DISTANCE('cosine', ds.embedding, @vector) AS [Distance]
                FROM dbo.Documents d
                INNER JOIN dbo.DocumentSummaryEmbeddings ds ON ds.id = d.id
                ORDER BY Distance ASC;
                """,
                new
                {
                    searchRequest.Query,
                    @k = searchRequest.Top
                })
                ).ToList();

            _logSearchResult(_logger, results.Count, null);

            return results;
        }
        catch (Exception ex)
        {
            _logQueryError(_logger, ex);
            throw;
        }
    }
}
