using Microsoft.EntityFrameworkCore;

namespace Si.Framework.EntityFramework.UnitofWork
{
    public class UnitOfWork<TContext> : IUnitOfWork, IDisposable where TContext:SiDbContext
    {
        private readonly TContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 获取指定实体的仓储
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>对应的仓储实例</returns>
        public IRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new Repository<T>(_context);
                _repositories[type] = repositoryInstance;
            }

            return (IRepository<T>)_repositories[type];
        }

        /// <summary>
        /// 提交所有更改，支持事务
        /// </summary>
        /// <returns>受影响的行数</returns>
        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 提交事务并处理异步操作
        /// </summary>
        /// <param name="action">需要在事务中执行的操作</param>
        public async Task ExecuteTransactionAsync(Func<Task> action)
        {
            await _context.ExecuteTransactionAsync(action);
        }

        /// <summary>
        /// 回滚当前上下文的所有更改
        /// </summary>
        public void Rollback()
        {
            var entries = _context.ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }

        /// <summary>
        /// 释放UnitOfWork持有的资源
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task ExecuteTransactionWithRetryAsync(Func<Task> action, int retryCount = 3)
        {
            return _context.ExecuteWithRetryTransactionAsync(action, retryCount);
        }
    }
}
