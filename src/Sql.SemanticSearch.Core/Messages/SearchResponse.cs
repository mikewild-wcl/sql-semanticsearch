using System.Text.Json.Serialization;

namespace Sql.SemanticSearch.Core.Messages;

public record SearchResponse()
{
    [JsonPropertyName("details")]
    public IReadOnlyCollection<string> Details { get; init; } = [];
}
