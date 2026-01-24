namespace Sql.SemanticSearch.Core.Configuration;

public record AISettings(
    string Provider,
    string ExternalEmbeddingModel)
{
    public int EmbeddingModelDimensions { get; init; }
}
