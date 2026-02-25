using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Sql.SemanticSearch.Core.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Shared;

namespace Sql.SemanticSearch.Core.Chunking;

/// Writes ingestion chunks to SQL Server, generating embeddings via AI_GENERATE_EMBEDDINGS.
public sealed class SqlServerChunkWriter(
    IDatabaseConnection databaseConnection,
    ResiliencePipelineProvider<string> resiliencePipelineProvider,
    AISettings aiSettings,
    ILogger<SqlServerChunkWriter> logger,
    VectorStoreWriterOptions? options = default) : IngestionChunkWriter<string>
{
    private readonly IDatabaseConnection _databaseConnection = databaseConnection;
    private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider = resiliencePipelineProvider;
    private readonly AISettings _aiSettings = aiSettings;
    private readonly ILogger<SqlServerChunkWriter> _logger = logger;
    private readonly VectorStoreWriterOptions _options = options ?? new VectorStoreWriterOptions();

    private static readonly Action<ILogger, int, int, Exception?> _logChunkSaved =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(0, nameof(SqlServerChunkWriter)),
            "Saved chunk {ChunkId} for document {DocumentId}.");

    public override async Task WriteAsync(
        IAsyncEnumerable<IngestionChunk<string>> chunks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline(ResiliencePipelineNames.SqlServerRetry);

        IReadOnlyList<int>? preExistingKeys = null;

        await foreach (var chunk in chunks.WithCancellation(cancellationToken))
        {
            var documentId = await LookupDocumentIdAsync(chunk.Document.Identifier, cancellationToken);
            preExistingKeys ??= await GetPreExistingChunksIdsAsync(documentId, cancellationToken).ConfigureAwait(false);

            await resiliencePipeline.ExecuteAsync(
                async ct =>
                {
                    using var connection = _databaseConnection.CreateConnection();
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    var chunkId = await SaveDocumentChunkAsync(documentId, chunk.Content, transaction);
                    await SaveDocumentChunkEmbeddingAsync(chunkId, transaction);
                    _logChunkSaved(_logger, chunkId, documentId, null);

                    transaction.Commit();
                },
                cancellationToken);
        }

        if (preExistingKeys?.Count > 0)
        {
            await DeleteAsync(preExistingKeys, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<int> LookupDocumentIdAsync(string arxivId, CancellationToken cancellationToken)
    {
        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline(ResiliencePipelineNames.SqlServerRetry);

        return await resiliencePipeline.ExecuteAsync(
            async ct =>
            {
                var id = await _databaseConnection.ExecuteScalarAsync<int?>(
                    "SELECT [Id] FROM dbo.Documents WHERE [ArxivId] = @ArxivId",
                    new { ArxivId = arxivId });

                return id ?? throw new InvalidOperationException(
                    $"Document with ArxivId '{arxivId}' not found in the database.");
            },
            cancellationToken);
    }

    private async Task<int> SaveDocumentChunkAsync(int documentId, string content, System.Data.IDbTransaction transaction) =>
        await _databaseConnection.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.DocumentChunks ([DocumentId], [Content])
            VALUES (@DocumentId, @Content);

            SELECT CAST(SCOPE_IDENTITY() as int);
            """,
            new { DocumentId = documentId, Content = content },
            transaction: transaction);

    /* Note: Embedding model is *NOT* a SQL injection risk, it must be hard-coded so we have to use the settings value. */
    private async Task SaveDocumentChunkEmbeddingAsync(int chunkId, System.Data.IDbTransaction transaction) =>
        await _databaseConnection.ExecuteAsync(
            $"""
            INSERT INTO dbo.DocumentChunkEmbeddings ([Id], [Embedding])
            SELECT @Id, AI_GENERATE_EMBEDDINGS([Content] USE MODEL {_aiSettings.ExternalEmbeddingModel})
            FROM dbo.DocumentChunks
            WHERE [Id] = @Id;
            """,
            new { Id = chunkId },
            transaction: transaction);

    private async Task<IReadOnlyList<int>> GetPreExistingChunksIdsAsync(int documentId, CancellationToken cancellationToken)
    {
        if (!_options.IncrementalIngestion)
        {
            return [];
        }

        List<int> keys = [];

        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline(ResiliencePipelineNames.SqlServerRetry);

        await resiliencePipeline.ExecuteAsync(
            async ct =>
            {
                var ids = await _databaseConnection.QueryAsync<int>(
                    "SELECT [Id] FROM dbo.DocumentChunks WHERE [DocumentId] = @DocumentId",
                    new { DocumentId = documentId });

                keys.AddRange(ids);
            },
            cancellationToken);

        return keys;
    }


    private async Task DeleteAsync(IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return;
        }

        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline(ResiliencePipelineNames.SqlServerRetry);
        await resiliencePipeline.ExecuteAsync(
            async context =>
            {
                using var connection = _databaseConnection.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                await _databaseConnection.ExecuteAsync(
                    """
                    DELETE FROM dbo.DocumentChunkEmbeddings
                    WHERE [Id] IN @Ids;

                    DELETE FROM dbo.DocumentChunks
                    WHERE [Id] IN @Ids;
                    """,
                    new { Ids = keys },
                    transaction: transaction);

                transaction.Commit();
            },
            cancellationToken);
    }
}
