using Dapper;
using Sql.SemanticSearch.Core.Data.TypeMappers;

namespace Sql.SemanticSearch.Core.Data;

public static class TypeHandlerRegistry
{
    public static void RegisterHandlers()
    {
        SqlMapper.AddTypeHandler(new UriTypeHandler());
    }
}
