using Microsoft.EntityFrameworkCore.Diagnostics;
using Si.EntityFramework.Extension.Database;
using System.Data.Common;
using System.Text;

namespace Si.EntityFramework.Extension.DataBase.Kits
{
    public class CommandAnalysisInterceptor<TContext> : DbCommandInterceptor where TContext : ApplicationDbContext
    {
        private readonly DbContextRouter<TContext> router;
        public CommandAnalysisInterceptor(DbContextRouter<TContext> router) : base()
        {
            router = router;
        }
        public override InterceptionResult<DbCommand> CommandCreating(
      CommandCorrelatedEventData eventData,
      InterceptionResult<DbCommand> result)
        {
            if (eventData.Context is ApplicationDbContext ctx)
            {
                var initialCommand = result.Result;
                if (initialCommand != null)
                {
                    router.IsReadOperation = !router.ForceDbMaster && IsReadCommand(initialCommand);
                    router.PendingConnection = router.IsReadOperation ? router.GetReadConnection() : router.GetWriteConnection();
                }
            }
            return base.CommandCreating(eventData, result);
        }
        private static bool IsReadCommand(DbCommand command)
        {
            var firstToken = GetFirstCommandToken(command.CommandText);
            return string.Equals(firstToken, "SELECT", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetFirstCommandToken(string sql)
        {
            using var reader = new StringReader(sql);
            var sb = new StringBuilder();
            while (true)
            {
                int c = reader.Read();
                if (c == -1) break;

                if (char.IsLetter((char)c))
                {
                    sb.Append((char)c);
                }
                else if (sb.Length > 0)
                {
                    break;
                }
            }
            return sb.ToString().ToUpperInvariant();
        }
    }
}
