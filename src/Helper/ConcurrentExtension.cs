using Microsoft.EntityFrameworkCore;

namespace Si.EntityFramework.Extension.Helper
{
    public static class ConcurrencyExtensions
    {
        /// <summary>
        /// 使用悲观锁获取实体
        /// </summary>
        public static async Task<T> GetWithLockAsync<T>(
            this DbContext context,
            object id,
            CancellationToken cancellationToken = default) where T : class
        {
            var query = context.Set<T>().AsQueryable();
            var providerName = context.Database.ProviderName?.ToLower();

            string sql = providerName switch
            {
                // SQL Server
                var x when x?.Contains("sqlserver") == true =>
                    $"SELECT * FROM {typeof(T).Name} WITH (UPDLOCK, ROWLOCK) WHERE Id = @p0",

                // PostgreSQL
                var x when x?.Contains("npgsql") == true =>
                    $"SELECT * FROM \"{typeof(T).Name}\" WHERE \"Id\" = @p0 FOR UPDATE",

                // MySQL/MariaDB
                var x when x?.Contains("mysql") == true || x?.Contains("mariadb") == true =>
                    $"SELECT * FROM `{typeof(T).Name}` WHERE `Id` = @p0 FOR UPDATE",

                // 不支持的数据库类型
                _ => throw new NotSupportedException($"Database provider '{providerName}' not supported for locking")
            };

            try
            {
                return await context.Database.SqlQueryRaw<T>(sql, new object[] { id })
                .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing lock query: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// 使用乐观锁更新实体
        /// </summary>
        public static async Task<bool> TryOptimisticUpdateAsync<T>(
            this DbContext context,
            T entity,
            Action<T> updateAction) where T : class
        {
            try
            {
                var entry = context.Entry(entity);
                updateAction(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}
