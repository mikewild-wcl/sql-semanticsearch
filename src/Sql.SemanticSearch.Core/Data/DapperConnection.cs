using Dapper;
using Microsoft.Data.SqlClient;
using Sql.SemanticSearch.Core.Data.Interfaces;
using System.Data;

namespace Sql.SemanticSearch.Core.Data;

public class DapperConnection(
    SqlConnection sqlConnection) : IDatabaseConnection
{
    private readonly SqlConnection _sqlConnection = sqlConnection;

    public async Task<int> ExecuteAsync(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
        {
            return await transaction.Connection!.ExecuteAsync(sql, param, transaction, commandType: commandType);
        }

        System.Diagnostics.Debug.WriteLine($"Connection: {_sqlConnection.ConnectionString}, state {_sqlConnection.State}");

        return await _sqlConnection.ExecuteAsync(sql, param, commandType: commandType);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.QueryAsync<T>(sql, param, transaction, commandType: commandType);
        }

        return await _sqlConnection.QueryAsync<T>(sql, param, commandType: commandType);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.QueryFirstOrDefaultAsync<T?>(sql, param, transaction, commandType: commandType);
        }

        return await _sqlConnection.QueryFirstOrDefaultAsync<T>(sql, param, commandType: commandType);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.ExecuteScalarAsync<T>(sql, param, transaction, commandType: commandType);
        }

        return await _sqlConnection.ExecuteScalarAsync<T>(sql, param, commandType: commandType);
    }
}
