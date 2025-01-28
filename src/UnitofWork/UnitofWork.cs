﻿using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Abstraction;
using Si.EntityFramework.Extension.DataBase;

namespace Si.EntityFramework.Extension.UnitofWork
{
    public class UnitOfWork<TContext> : IUnitOfWork, IDisposable where TContext : ApplicationDbContext
    {
        private readonly TContext _context;
        private static Dictionary<Type, object> _repositories = new();

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
        /// 释放UnitOfWork持有的资源
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
