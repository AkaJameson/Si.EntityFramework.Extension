using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Abstraction;
using Si.EntityFramework.Extension.Entitys;
using Si.EntityFramework.Extension.Kits;
using System.Linq.Expressions;

namespace Si.EntityFramework.Extension.DataBase
{
    public class ApplicationDbContext : DbContext
    {
        internal readonly IdGenerator _idGenerator;
        protected internal readonly ExtensionDbOptions _siDbContextOptions;
        internal readonly IUserInfo sessions;
        protected ApplicationDbContext(
            DbContextOptions options, 
            ExtensionDbOptions siOptions, 
            IUserInfo sessions = null)
            : base(options)
        {
            _siDbContextOptions = siOptions;
            this.sessions = sessions;
            if (_siDbContextOptions.EnableSnowflakeId)
            {
                _idGenerator = new IdGenerator(_siDbContextOptions.WorkerId, _siDbContextOptions.DatacenterId);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (_siDbContextOptions.EnableMultiTenant)
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType) &&
                        !_siDbContextOptions.IgnoredMultiTenantTypes.Contains(entityType.ClrType))
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var tenantProperty = Expression.Property(parameter, nameof(IMultiTenant.TenantId));
                        var tenantValue = Expression.Constant(sessions?.TenantId);
                        var comparison = Expression.Equal(tenantProperty, tenantValue);
                        var lambda = Expression.Lambda(comparison, parameter);
                        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
        }

        public override int SaveChanges()
        {
            ApplyFeatures();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyFeatures();
            return base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyFeatures();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        private void ApplyFeatures()
        {
            if (_siDbContextOptions.EnableSnowflakeId)
            {
                ApplySnowflakeId();
            }

            if (_siDbContextOptions.EnableSoftDelete)
            {
                UpdateSoftDeleteState();
            }

            if (_siDbContextOptions.EnableAudit)
            {
                ApplyAuditInfo();
            }
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

        private void UpdateSoftDeleteState()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is ISoftDelete softDelete)
                {
                    switch (entry.State)
                    {
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            softDelete.IsDeleted = true;
                            softDelete.DeletedTime = DateTime.Now;
                            break;
                    }
                }
            }
        }

        private void ApplyAuditInfo()
        {
            var userId = sessions?.UserId.ToString() ?? "System";
            var entries = ChangeTracker.Entries().ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is ICreationAudited creationAudited && entry.State == EntityState.Added)
                {
                    creationAudited.CreatedBy = userId;
                    creationAudited.CreatedTime = DateTime.Now;
                }

                if (entry.Entity is IModificationAudited modificationAudited && entry.State == EntityState.Modified)
                {
                    modificationAudited.LastModifiedBy = userId;
                    modificationAudited.LastModifiedTime = DateTime.Now;
                }

                if (entry.Entity is IFullAudited fullAudited && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    fullAudited.IsDeleted = true;
                    fullAudited.DeletedTime = DateTime.Now;
                    fullAudited.DeletedBy = userId;
                }
            }
        }
    }
}
