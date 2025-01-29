using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Si.EntityFramework.Extension.Rbac.Entitys
{
    public class UserRole
    {
        public long UserId { get; set;}
        public int RoleId { get; set; }
    }
    public class RoleUserConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(x => new { x.UserId, x.RoleId });
        }
    }
}
