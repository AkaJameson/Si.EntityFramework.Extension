using Si.EntityFramework.Extension.DataBase.Abstraction;

namespace Si.EntityFramework.Extension.DataBase.Entitys
{
    public class UserInfo : IUserInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 租户ID
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public List<string> Roles { get; set; } = new();
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
    }
}
