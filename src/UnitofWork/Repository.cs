using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Si.EntityFramework.Extension.Abstraction;
using Si.EntityFramework.Extension.DataBase;
using Si.EntityFramework.Extension.Entitys;
using System.Linq.Expressions;

namespace Si.EntityFramework.Extension.UnitofWork
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SiDbContext _dbContext;
        protected readonly DbSet<T> DbSet;
        protected readonly SiDbContextOptions _options;
        public Repository(SiDbContext dbContext)
        {
            _dbContext = dbContext;
            DbSet = _dbContext.Set<T>();
            _options = (_dbContext as SiDbContextBase)?._siDbContextOptions ?? new SiDbContextOptions();
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


        public async Task<bool> IsSoftDeleteEnabled()
        {
            return _options.EnableSoftDelete && typeof(ISoftDelete).IsAssignableFrom(typeof(T));
        }

        public async Task SoftDeleteAsync(T entity)
        {
            if (!await IsSoftDeleteEnabled())
            {
                return;
            }
            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                softDelete.DeletedTime = DateTime.Now;
                if (entity is IFullAudited fullAudited && _dbContext is SiDbContextBase siContext)
                {
                    fullAudited.DeletedBy = siContext._currentUser?.UserId.ToString() ?? "System";
                }
                await UpdateAsync(entity);
            }
        }

        public async Task SoftDeleteRangeAsync(IEnumerable<T> entities)
        {
            if (!await IsSoftDeleteEnabled())
            {
                await DeleteRangeAsync(entities);
                return;
            }

            foreach (var entity in entities)
            {
                await SoftDeleteAsync(entity);
            }
        }

        public async Task RestoreAsync(T entity)
        {
            if (!await IsSoftDeleteEnabled())
            {
                throw new InvalidOperationException("Soft delete is not enabled for this entity.");
            }

            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = false;
                softDelete.DeletedTime = null;

                if (entity is IFullAudited fullAudited)
                {
                    fullAudited.DeletedBy = null;
                }

                await UpdateAsync(entity);
            }
        }

        public async Task RestoreRangeAsync(IEnumerable<T> entities)
        {
            if (!await IsSoftDeleteEnabled())
            {
                throw new InvalidOperationException("Soft delete is not enabled for this entity.");
            }

            foreach (var entity in entities)
            {
                await RestoreAsync(entity);
            }
        }

        public IQueryable<T> GetAllIncludeDeleted()
        {
            if (!_options.EnableSoftDelete || !typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                return DbSet;
            }

            // 移除软删除过滤器
            return DbSet.IgnoreQueryFilters();
        }

        public async Task<int> SaveRepository(CancellationToken cancellationToken = default)
        {
            var entries = _dbContext.ChangeTracker.Entries().ToList();
            var otherEntries = entries
                .Where(e => e.Entity.GetType() != typeof(T))
                .ToList();
            var originalStates = new Dictionary<EntityEntry, EntityState>();

            try
            {
                foreach (var entry in otherEntries)
                {
                    originalStates[entry] = entry.State;
                    entry.State = EntityState.Unchanged;
                }

                // 仅当启用相应功能时才应用特性
                if (_dbContext is SiDbContextBase siContext)
                {
                    var currentTypeEntries = entries
                        .Where(e => e.Entity.GetType() == typeof(T))
                        .ToList();

                    // 应用雪花ID
                    if (_options.EnableSnowflakeId)
                    {
                        foreach (var entry in currentTypeEntries.Where(e =>
                            e.State == EntityState.Added && e.Entity is ISnowflakeId))
                        {
                            if (entry.Entity is ISnowflakeId entity)
                            {
                                entity.Id = siContext._idGenerator.Fetch();
                            }
                        }
                    }

                    // 应用软删除
                    if (_options.EnableSoftDelete)
                    {
                        foreach (var entry in currentTypeEntries.Where(e =>
                            e.State == EntityState.Deleted && e.Entity is ISoftDelete))
                        {
                            if (entry.Entity is ISoftDelete softDelete)
                            {
                                entry.State = EntityState.Modified;
                                softDelete.IsDeleted = true;
                                softDelete.DeletedTime = DateTime.Now;
                            }
                        }
                    }

                    // 应用审计
                    if (_options.EnableAudit)
                    {
                        var userId = siContext._currentUser?.UserId.ToString() ?? "System";

                        foreach (var entry in currentTypeEntries)
                        {
                            if (entry.Entity is ICreationAudited creationAudited &&
                                entry.State == EntityState.Added)
                            {
                                creationAudited.CreatedBy = userId;
                                creationAudited.CreatedTime = DateTime.Now;
                            }

                            if (entry.Entity is IModificationAudited modificationAudited &&
                                entry.State == EntityState.Modified)
                            {
                                modificationAudited.LastModifiedBy = userId;
                                modificationAudited.LastModifiedTime = DateTime.Now;
                            }

                            if (entry.Entity is IFullAudited fullAudited &&
                                entry.State == EntityState.Deleted)
                            {
                                fullAudited.DeletedBy = userId;
                            }
                        }
                    }
                }

                return await _dbContext.SaveChangesAsync(cancellationToken);
            }
            finally
            {
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
