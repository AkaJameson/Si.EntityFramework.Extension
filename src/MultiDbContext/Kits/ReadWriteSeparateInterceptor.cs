using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Si.EntityFramework.Extension.MultiDbContext.Kits
{
    public class ReadWriteSeparateInterceptor
    {
        public class ReadWriteSeparateInterceptor : DbCommandInterceptor
        {
            private readonly MultiDbContextRouter _context;

            public ReadWriteSeparateInterceptor(MultiDbContextRouter context)
            {
                _context = context;
            }

            public override InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                if (IsWriteOperation(command))
                {
                    _context.UseMaster();
                }
                return base.ReaderExecuting(command, eventData, result);
            }

            private bool IsWriteOperation(DbCommand command)
            {
                return command.CommandText.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) ||
                       command.CommandText.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                       command.CommandText.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase);
            }
        }

    }
}
