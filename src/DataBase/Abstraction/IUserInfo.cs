namespace Si.EntityFramework.Extension.DataBase.Abstraction
{
    public interface IUserInfo
    {
        List<string> Roles { get; set; }
        string TenantId { get; set; }
        long UserId { get; set; }
        string UserName { get; set; }
    }
}