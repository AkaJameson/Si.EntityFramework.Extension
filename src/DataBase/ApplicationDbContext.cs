using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.DataBase;
using Si.EntityFramework.Extension.DataBase.Abstraction;
using Si.EntityFramework.Extension.DataBase.Configuration;
using Si.EntityFramework.Extension.DataBase.Kits;
using System.Linq.Expressions;

namespace Si.EntityFramework.Extension.Database
{
    public class ApplicationDbContext : DbContext
    {
        internal readonly IdGenerator _idGenerator;
        protected internal readonly ExDbOptions exOptions;
        internal readonly IUserInfo userInfo;
        private readonly IServiceProvider sp;
        protected ApplicationDbContext(
            DbContextOptions options, IServiceProvider sp,
            IUserInfo sessions = null)
            : base(options)
        {
            var typeName = this.GetType().Name;
            exOptions = DbStartUp.ExOptions.TryGetValue(typeName, out exOptions) ? exOptions : new ExDbOptions();
            this.userInfo = sessions;
            if (exOptions.EnableSnowflakeId)
            {
                _idGenerator = new IdGenerator(exOptions.WorkerId, exOptions.DatacenterId);
            }
            this.sp = sp;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (exOptions.EnableMultiTenant)
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType) &&
                        !exOptions.IgnoredMultiTenantTypes.Contains(entityType.ClrType))
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var tenantProperty = Expression.Property(parameter, nameof(IMultiTenant.TenantId));
                        var tenantValue = Expression.Constant(userInfo?.TenantId);
                        var comparison = Expression.Equal(tenantProperty, tenantValue);
                        var lambda = Expression.Lambda(comparison, parameter);
                        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
        }
        public TContext ForceMaster<TContext>() where TContext : ApplicationDbContext
        {
            var contextType = this.GetType();
            var router = sp.GetService<DbContextRouter<TContext>>();
            if (router != null)
            {
                router.ForceMaster();
            }
            return (TContext)this;


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
            if (exOptions.EnableSnowflakeId)
            {
                ApplySnowflakeId();
            }

            if (exOptions.EnableSoftDelete)
            {
                UpdateSoftDeleteState();
            }

            if (exOptions.EnableAudit)
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
            var userId = userInfo?.UserId.ToString() ?? "System";
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
