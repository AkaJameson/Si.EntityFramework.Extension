namespace Si.EntityFramework.Extension.Abstraction
{
    public interface IMultiTenant
    {
        string TenantId { get; set; }
    }
}