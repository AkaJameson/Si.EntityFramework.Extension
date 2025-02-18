using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Si.EntityFramework.Extension.Abstraction;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.Extensions;

namespace Si.EntityFramework.Extension.UnitofWork
{
    public class UnitOfWork<TContext> : IDisposable, IUnitOfWork where TContext : ApplicationDbContext
    {
        private readonly TContext _context;
        private Dictionary<Type, object> _repositories = new();
        private IDbContextTransaction _currentTransaction;
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
        /// 提交所有更改
        /// </summary>
        /// <returns>受影响的行数</returns>
        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
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
        /// 执行原生SQL查询
        /// </summary>
        public async Task<IEnumerable<T>> SqlQueryAsync<T>(string sql, params object[] parameters) where T : class, new()
        {
            return await _context.Database.FromSqlCollectionAsync<T>(sql, parameters);
        }

        /// <summary>
        /// 执行原生SQL命令
        /// </summary>
        public async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
        /// <summary>
        /// 开始一个新的事务
        /// </summary>
        public void BeginTransaction()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("Transaction already started.");

            _currentTransaction = _context.Database.BeginTransaction();
        }

        /// <summary>
        /// 提交当前事务
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No transaction started.");

            await _context.SaveChangesAsync();
            await _currentTransaction.CommitAsync();
        }

        /// <summary>
        /// 回滚当前事务
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No transaction started.");

            await _currentTransaction.RollbackAsync();
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
        /// <summary>
        /// 清除上下文跟踪记录
        /// </summary>
        public void ClearChangeTracker()
        {
            _context.ChangeTracker.Clear();
        }

        /// <summary>
        /// 释放UnitOfWork持有的资源
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
