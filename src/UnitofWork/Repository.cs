using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Si.Framework.EntityFramework.UnitofWork
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly SiDbContext _dbContext;
        private readonly DbSet<T> DbSet;

        public Repository(SiDbContext dbContext)
        {
            _dbContext = dbContext;
            DbSet = _dbContext.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.Where(predicate).ToListAsync();
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.SingleOrDefaultAsync(predicate);
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true)
        {
            IQueryable<T> query = DbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            int totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await DbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await DbSet.AddRangeAsync(entities);
        }

        public Task ForceUpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var entry = _dbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                throw new InvalidOperationException("Entity must be attached to the context before it can be updated.");
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task ForceUpdateRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            foreach (var entity in entities)
            {
                // 检查每个实体是否被追踪
                var entry = _dbContext.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    throw new InvalidOperationException("One or more entities are not being tracked by the context.");
                }
            }
            DbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await DbSet.Where(predicate).ToListAsync();
            DbSet.RemoveRange(entities);
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            DbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await DbSet.CountAsync();

            return await DbSet.CountAsync(predicate);
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public Task UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            DbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public async Task<int> SaveRepository(CancellationToken cancellationToken = default)
        {
            var entries = _dbContext.ChangeTracker.Entries().ToList();
            // 暂时将不相关的实体标记为Unchanged
            var otherEntries = entries
                .Where(e => e.Entity.GetType() != typeof(T))
                .ToList();
            var originalStates = new Dictionary<EntityEntry, EntityState>();
            try
            {
                // 保存其他实体的原始状态并将其设置为Unchanged
                foreach (var entry in otherEntries)
                {
                    originalStates[entry] = entry.State;
                    entry.State = EntityState.Unchanged;
                }
                var result = await _dbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
            finally
            {
                // 恢复其他实体的原始状态
                foreach (var entry in otherEntries)
                {
                    if (originalStates.ContainsKey(entry))
                    {
                        entry.State = originalStates[entry];
                    }
                }
            }

        }
    }
}
