using System.Text.Json;

namespace Sql.SemanticSearch.Core.Serialization;

internal static class SerializerOptions
{
    public static readonly JsonSerializerOptions CamelCaseSerialierOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
