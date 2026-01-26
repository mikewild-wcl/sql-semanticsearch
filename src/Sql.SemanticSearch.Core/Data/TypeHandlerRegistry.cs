using Dapper;
using Sql.SemanticSearch.Core.Data.TypeHandlers;
using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.Core.Data;

[ExcludeFromCodeCoverage]
public static class TypeHandlerRegistry
{
    public static void RegisterHandlers()
    {
        SqlMapper.AddTypeHandler(new UriTypeHandler());
        SqlMapper.AddTypeHandler(new MetadataTypeHandler());
    }
}
