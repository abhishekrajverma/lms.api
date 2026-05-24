using Dapper;
using EMS.Shared.Interfaces.Repositories;
using EMS.Shared.Interfaces.SqlExecutor;
using System.Data;

public class SqlExecutor : ISqlExecutor
{
    private readonly IDbConnectionFactory _factory;

    public SqlExecutor(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    private IDbConnection CreateConnection() => _factory.CreateConnection();

    public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? tx = null)
    {
        using var conn = tx?.Connection ?? CreateConnection();
        return await conn.ExecuteAsync(sql, param, tx);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? tx = null)
    {
        using var conn = tx?.Connection ?? CreateConnection();
        return await conn.ExecuteScalarAsync<T>(sql, param, tx);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? tx = null)
    {
        using var conn = tx?.Connection ?? CreateConnection();
        return await conn.QueryAsync<T>(sql, param, tx);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? tx = null)
    {
        using var conn = tx?.Connection ?? CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(sql, param, tx);
    }

    // One-to-One or One-to-Many base mapping
    public async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, TReturn>(
        string sql,
        Func<T1, T2, TReturn> map,
        object? param = null,
        string splitOn = "Id",
        IDbTransaction? tx = null)
    {
        using var conn = tx?.Connection ?? CreateConnection();
        return await conn.QueryAsync(sql, map, param, tx, splitOn: splitOn);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, TReturn>(
        string sql,
        Func<T1, T2, T3, TReturn> map,
        object? param = null,
        string splitOn = "Id",
        IDbTransaction? tx = null)
    {
        using var conn = tx?.Connection ?? CreateConnection();
        return await conn.QueryAsync(sql, map, param, tx, splitOn: splitOn);
    }

    // Multiple result sets
    public async Task<(IEnumerable<T1>, IEnumerable<T2>)> QueryMultipleAsync<T1, T2>(
        string sql,
        object? param = null,
        IDbTransaction? tx = null)
    {
        using var conn = tx?.Connection ?? CreateConnection();
        using var multi = await conn.QueryMultipleAsync(sql, param, tx);

        var result1 = await multi.ReadAsync<T1>();
        var result2 = await multi.ReadAsync<T2>();

        return (result1, result2);
    }

    // Transaction wrapper
    public async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            await action(tx);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
