using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Si.EntityFramework.PermGuard.Entitys
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //懒加载
        public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<UserBase> Users { get; set; }
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(200);
            builder.HasMany(x => x.Users).WithMany(x => x.Roles).UsingEntity(x => x.ToTable("RoleUser"));
            builder.HasMany(x => x.Permissions).WithMany(x => x.Roles).UsingEntity(x => x.ToTable("RolePermission"));
        }
    }
}
