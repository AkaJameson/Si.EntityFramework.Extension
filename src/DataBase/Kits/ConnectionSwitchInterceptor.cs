using Microsoft.EntityFrameworkCore.Diagnostics;
using Si.EntityFramework.Extension.Database;
using System.Data.Common;

namespace Si.EntityFramework.Extension.DataBase.Kits
{
    public class ConnectionSwitchInterceptor<TContext> : DbConnectionInterceptor where TContext : ApplicationDbContext
    {
        private readonly DbContextRouter<TContext> router;
        public ConnectionSwitchInterceptor(DbContextRouter<TContext> router) : base()
        {
            router = router;
        }
        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            if (eventData.Context is ApplicationDbContext ctx &&
                router.PendingConnection != null)
            {
                // 同步连接字符串
                connection.ConnectionString = router.PendingConnection;
                router.PendingConnection = null; // 重置状态
            }
            return base.ConnectionOpening(connection, eventData, result);
        }
        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is ApplicationDbContext ctx &&
              router.PendingConnection != null)
            {
                // 同步连接字符串
                connection.ConnectionString = router.PendingConnection;
                router.PendingConnection = null; // 重置状态
            }
            return await base.ConnectionOpeningAsync(
                connection, eventData, result, cancellationToken);
        }
    }

}
