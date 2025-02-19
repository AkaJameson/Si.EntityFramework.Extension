using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Si.EntityFramework.Extension.MultiDbContext.Kits
{
    public class ConnectionSwitchInterceptor :DbConnectionInterceptor
    {
        public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
        {
            if (eventData.Context is MutiContext ctx)
            {
                var newConnection = ctx.IsReadOperation
                    ? ctx.GetReadConnection()
                    : ctx.GetWriteConnection();

                // 实际切换连接的核心逻辑
                connection.ConnectionString = newConnection.ConnectionString;
            }
            return base.ConnectionOpening(connection, eventData, result);
        }
    }
}
