using System.Text.Json;

namespace Sql.SemanticSearch.Core.Serialization;

public static class SerializerOptions
{
    public static readonly JsonSerializerOptions CamelCaseSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly JsonSerializerOptions DefaultWebSerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
}
