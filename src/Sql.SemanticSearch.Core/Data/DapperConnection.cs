using Dapper;
using Sql.SemanticSearch.Core.Data.Interfaces;
using System.Data;

namespace Sql.SemanticSearch.Core.Data;

public class DapperConnection(
    Func<IDbConnection> connectionFactory) : IDatabaseConnection
{
    private readonly Func<IDbConnection> _connectionFactory = connectionFactory;

    public IDbConnection CreateConnection() => _connectionFactory();

    public async Task<int> ExecuteAsync(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
        {
            return await transaction.Connection!.ExecuteAsync(sql, param, transaction, commandType: commandType);
        }

        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, param, commandType: commandType);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.QueryAsync<T>(sql, param, transaction, commandType: commandType);
        }

        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(sql, param, commandType: commandType);    
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.QueryFirstOrDefaultAsync<T?>(sql, param, transaction, commandType: commandType);
        }

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param, commandType: commandType);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.ExecuteScalarAsync<T>(sql, param, transaction, commandType: commandType);
        }

        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<T>(sql, param, commandType: commandType);
    }
}
