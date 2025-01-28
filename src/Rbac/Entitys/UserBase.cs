namespace Si.EntityFramework.PermGuard.Entitys
{
    public abstract class UserBase
    {
        public long Id { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }
}
