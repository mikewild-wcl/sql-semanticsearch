using Dapper;
using Sql.SemanticSearch.Core.Data;
using Sql.SemanticSearch.Core.Data.TypeMappers;

namespace Sql.SemanticSearch.Core.UnitTests.Data;

public class TypeHandlerRegistryTests
{
    [Fact]
    public void RegisterHandlers_RegistersUriTypeHandler()
    {
        // Arrange & Act
        //TypeHandlerRegistry.RegisterHandlers();

        SqlMapper.HasTypeHandler(typeof(Uri));
        SqlMapper.HasTypeHandler(typeof(UriTypeHandler));
        SqlMapper.HasTypeHandler(typeof(Search.SearchServiceTests));
    }
}