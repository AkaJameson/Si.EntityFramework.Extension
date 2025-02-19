namespace Si.EntityFramework.Extension.DataBase.Abstraction
{
    public interface IMultiTenant
    {
        string TenantId { get; set; }
    }
}