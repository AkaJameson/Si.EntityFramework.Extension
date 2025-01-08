using Microsoft.EntityFrameworkCore;
using Si.Framework.EntityFramework.Abstraction;
using Si.Framework.EntityFramework.Kit;

namespace Si.Framework.EntityFramework
{
    public class SiDbContextBase:DbContext
    {
        protected readonly IdGenerator _idGenerator;
        protected SiDbContextBase(DbContextOptions options, IdGenerator idGenerator = null)
            : base(options)
        {
            _idGenerator = idGenerator;
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySnowflakeId();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplySnowflakeId()
        {
            if (_idGenerator == null) return;

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is ISnowflakeId);

            foreach (var entry in entries)
            {
                if (entry.Entity is ISnowflakeId entity)
                {
                    entity.Id = _idGenerator.Fetch();
                }
            }
        }
    }
}
