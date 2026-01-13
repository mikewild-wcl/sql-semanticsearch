namespace Sql.SemanticSearch.DatabaseDeployment;

internal static class EnvironmentVariables
{
    public const string AIProvider = "AI_PROVIDER";
    public const string DefaultAIProvider = "OLLAMA";

    public const string AIEndpoint = "AI_CLIENT_ENDPOINT";    
    public const string AIClientKey = "AI_CLIENT_KEY";
    
    public const string EmbeddingModel = "EMBEDDING_MODEL";
    public const string EmbeddingDimensions = "EMBEDDING_DIMENSIONS";

    public const string OllamaTunnelEndpoint = "OLLAMA_HTTP";

}
