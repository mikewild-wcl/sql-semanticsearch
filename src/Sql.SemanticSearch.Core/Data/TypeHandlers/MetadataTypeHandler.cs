using Dapper;
using Sql.SemanticSearch.Core.Search;
using Sql.SemanticSearch.Core.Serialization;
using System.Data;
using System.Text.Json;

namespace Sql.SemanticSearch.Core.Data.TypeHandlers;

internal class MetadataTypeHandler : SqlMapper.TypeHandler<DocumentMetadata>
{
    public override void SetValue(IDbDataParameter parameter, DocumentMetadata? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value is not null 
            ? JsonSerializer.Serialize(value, SerializerOptions.CamelCaseSerialierOptions) 
            : DBNull.Value;
    }

    public override DocumentMetadata Parse(object value)
    {
        var json = value as string;
        var result = !string.IsNullOrWhiteSpace(json)
            ? JsonSerializer.Deserialize<DocumentMetadata>(json, SerializerOptions.CamelCaseSerialierOptions)
            : null;
        return result ?? new();
    }
}
