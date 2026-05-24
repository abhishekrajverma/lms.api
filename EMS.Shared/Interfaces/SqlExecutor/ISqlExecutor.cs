using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Shared.Interfaces.SqlExecutor
{
    public interface ISqlExecutor
    {
        Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? tx = null);
        Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? tx = null);

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? tx = null);
        Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? tx = null);

        Task<IEnumerable<TReturn>> QueryAsync<T1, T2, TReturn>(
            string sql,
            Func<T1, T2, TReturn> map,
            object? param = null,
            string splitOn = "Id",
            IDbTransaction? tx = null);

        Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, TReturn>(
            string sql,
            Func<T1, T2, T3, TReturn> map,
            object? param = null,
            string splitOn = "Id",
            IDbTransaction? tx = null);

        Task<(IEnumerable<T1>, IEnumerable<T2>)> QueryMultipleAsync<T1, T2>(
            string sql,
            object? param = null,
            IDbTransaction? tx = null);

        Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action);
    }

}
