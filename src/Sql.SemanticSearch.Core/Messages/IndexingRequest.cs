using System.Text.Json.Serialization;

namespace Sql.SemanticSearch.Core.Messages;

public record IndexingRequest()
{
    [JsonPropertyName("ids")]
    public IReadOnlyCollection<string> Ids { get; init; } = [];
}
