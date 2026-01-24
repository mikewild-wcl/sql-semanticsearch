using System.Data;

namespace Sql.SemanticSearch.Core.Data.Interfaces;

public interface IDatabaseConnection
{
    Task CloseConnection();

    Task OpenConnection();

    IDbTransaction BeginTransaction();

    Task<int> ExecuteAsync(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);

    Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);

    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);

    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null);
}
