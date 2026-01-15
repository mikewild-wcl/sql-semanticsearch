using Dapper;
using Microsoft.Data.SqlClient;
using Sql.SemanticSearch.Core.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Sql.SemanticSearch.Core.Data;

public class DapperConnection(
    //string connectionString,
    SqlConnection sqlConnection) : IDatabaseConnection
{
    //private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    private readonly SqlConnection _sqlConnection = sqlConnection;

    public async Task<int> ExecuteAsync(string sql, object param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
        {
            return await transaction.Connection!.ExecuteAsync(sql, param, transaction, commandType: commandType);
        }

        System.Diagnostics.Debug.WriteLine($"Connection: {_sqlConnection.ConnectionString}, state {sqlConnection.State}");

        //using var connection = new SqlConnection(_connectionString);
        return await sqlConnection.ExecuteAsync(sql, param, commandType: commandType);
    }

    /*
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.QueryAsync<T>(sql, param, transaction, commandType: commandType);
        }

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<T>(sql, param, commandType: commandType);
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandType: commandType);
        }

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param, commandType: commandType);
    }

    public async Task<int> ExecuteAsync(string sql, object param = null, CommandType? commandType = null, IDbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.ExecuteAsync(sql, param, transaction, commandType: commandType);
        }

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(sql, param, commandType: commandType);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, CommandType? commandType = null, IDbTransaction transaction = null)
    {
        if (transaction != null)
        {
            return await transaction.Connection.ExecuteScalarAsync<T>(sql, param, transaction, commandType: commandType);
        }

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<T>(sql, param, commandType: commandType);
    }
    */
}

