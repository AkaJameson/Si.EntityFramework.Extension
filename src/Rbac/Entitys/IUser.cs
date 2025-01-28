namespace Si.EntityFramework.PermGuard.Entitys
{
    public interface IUser
    {
        public int Id { get; set; }
        public ICollection<Role> Roles { get; set; }
    }
}
