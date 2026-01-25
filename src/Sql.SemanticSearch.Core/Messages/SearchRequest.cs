using System.Text.Json.Serialization;

namespace Sql.SemanticSearch.Core.Messages;

public record SearchRequest()
{
    [JsonPropertyName("query")]
    public string Query { get; init; }

    [JsonPropertyName("top_k")]
    public int Top { get; init; } = 5;
}
