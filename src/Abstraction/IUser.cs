using Si.EntityFramework.PermGuard.Entitys;

namespace Si.EntityFramework.Extension.Abstraction
{
    /// <summary>
    /// RBAC
    /// </summary>
    public interface IUser
    {
        public long Id { get; set; }
    }
}
