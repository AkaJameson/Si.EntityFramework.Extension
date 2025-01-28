namespace Si.EntityFramework.PermGuard.Handlers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PermissionAttribute : Attribute
    {
        public string PermissionName { get; set; }

        public PermissionAttribute(string permissionName)
        {
            PermissionName = permissionName ?? string.Empty;
        }
    }
}
