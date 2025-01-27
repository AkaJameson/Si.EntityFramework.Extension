public interface ICurrentTenant
{
    string? TenantId { get; }
}
public interface IMultiTenant
{
    string TenantId { get; set; }
} 