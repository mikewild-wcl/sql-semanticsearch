using System.Data;

namespace Sql.SemanticSearch.Core.Data.Interfaces;

#pragma warning disable CA1040 // Avoid empty interfaces
public interface IDatabaseConnection
#pragma warning restore CA1040 // Avoid empty interfaces
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);
    
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);
    
    Task<int> ExecuteAsync(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);
    
    Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);
}
