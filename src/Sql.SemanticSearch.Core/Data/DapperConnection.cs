using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Sql.SemanticSearch.Core.Data.Interfaces;
using Sql.SemanticSearch.Shared;
using System.Data;

namespace Sql.SemanticSearch.Core.Data;

public class DapperConnection(
    IConfiguration configuration,
    Func<IDbConnection> connectionFactory,
    SqlConnection sqlConnection) : IDatabaseConnection
{
    private readonly SqlConnection _sqlConnection = sqlConnection;
    private readonly Func<IDbConnection> _connectionFactory = connectionFactory;

    private readonly string _connectionString = configuration.GetConnectionString(ResourceNames.SqlDatabase)
        ?? throw new InvalidOperationException($"Connection string {ResourceNames.SqlDatabase} not found");

    public IDbConnection CreateConnection()
    {
        if (connectionFactory is not null)
        {
            var connection =  _connectionFactory() as SqlConnection;
            if (connection is not null)
            {
                return connection;
            }
        }

        return new SqlConnection(_connectionString);        
    }

    public async Task CloseConnection()
    {
        await _sqlConnection.CloseAsync();
    }

    public async Task OpenConnection()
    {
        await _sqlConnection.OpenAsync();
    }

    public IDbTransaction BeginTransaction()
    {
        using var connection = CreateConnection();
        return connection.BeginTransaction();
        //return _sqlConnection.BeginTransaction();
    }

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
        return await _sqlConnection.QueryFirstOrDefaultAsync<T>(sql, param, commandType: commandType);
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
