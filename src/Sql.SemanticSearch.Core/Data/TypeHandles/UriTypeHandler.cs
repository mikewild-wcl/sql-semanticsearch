using Dapper;
using System.Data;

namespace Sql.SemanticSearch.Core.Data.TypeMappers;

internal class UriTypeHandler : SqlMapper.TypeHandler<Uri?>
{
    public override void SetValue(IDbDataParameter parameter, Uri? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value is not null ? value.ToString() : DBNull.Value;
    }

    public override Uri? Parse(object value)
    {
        var uriString = value?.ToString();

        return !string.IsNullOrWhiteSpace(uriString) && 
            Uri.TryCreate(value?.ToString(), UriKind.RelativeOrAbsolute, 
            out var uri) ? uri : null;
    }
}
