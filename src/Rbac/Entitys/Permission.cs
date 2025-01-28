using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Si.EntityFramework.PermGuard.Entitys
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
            builder.HasIndex(p => p.Name).IsUnique();
            builder.Property(p => p.Description).HasMaxLength(256);
        }
    }
}
