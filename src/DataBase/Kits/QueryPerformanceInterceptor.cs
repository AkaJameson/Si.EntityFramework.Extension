using Microsoft.EntityFrameworkCore.Diagnostics;
using Si.EntityFramework.Extension.DataBase.Entitys;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;

namespace Si.EntityFramework.Extension.DataBase.Kits
{
    public class QueryPerformanceInterceptor : IInterceptor
    {
        private readonly ConcurrentDictionary<string, QueryMetrics> _metrics = new();
        public virtual InterceptionResult<DbDataReader> ReaderExecuting(
       DbCommand command,
       CommandEventData eventData,
       InterceptionResult<DbDataReader> result)
        {
            var stopwatch = Stopwatch.StartNew();

            return result;
        }
        public virtual async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
        {
            var sql = command.CommandText;
            var metrics = _metrics.GetOrAdd(sql, _ => new QueryMetrics());

            metrics.ExecutionCount++;
            metrics.LastExecuted = DateTime.UtcNow;

            var stopwatch = Stopwatch.StartNew();
            try
            {
                return result;
            }
            finally
            {
                stopwatch.Stop();
                metrics.TotalDuration += stopwatch.Elapsed;
            }
        }

        public IEnumerable<(string Sql, QueryMetrics Metrics)> GetMetrics()
        {
            return _metrics.Select(x => (x.Key, x.Value));
        }
    }
}
