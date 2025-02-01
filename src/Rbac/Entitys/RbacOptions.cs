using System.Data;
using System.Xml.Linq;

namespace Si.EntityFramework.Extension.Rbac.Entitys
{
    public class RbacOptions
    {
        internal List<Permission> Permissions { get; set; } = new List<Permission>();
        internal List<Role> Roles { get; set; } = new List<Role>();
        public string ConfigPath { get; set; } = "rbac.json";
        public string SecrectKey { get; set; } = "M1D9K3dMWVFrJ7ZKLYATA7f9zy9VYf5a";
        public string Issuer { get; set; } = "si.permguard.issuer";
        public string Audience { get; set; } = "si.permguard.audience";
        internal void LoadFromXml()
        {
            if (!File.Exists(ConfigPath))
            {
                throw new FileNotFoundException($"Config file {ConfigPath} not found.");
            }
            // 加载XML文档
            var doc = XDocument.Load(ConfigPath);
            // 加载权限数据
            var permissionsElement = doc.Element("Rbac")?.Element("Permissions");
            if (permissionsElement != null)
            {
                foreach (var permElement in permissionsElement.Elements("Permission"))
                {
                    var permission = new Permission
                    {
                        Id = int.Parse(permElement.Element("Id")?.Value),
                        Name = permElement.Element("Name")?.Value,
                        Description = permElement.Element("Description")?.Value
                    };
                    Permissions.Add(permission);
                }
            }
            // 加载角色数据
            var rolesElement = doc.Element("Rbac")?.Element("Roles");
            if (rolesElement != null)
            {
                foreach (var roleElement in rolesElement.Elements("Role"))
                {
                    var role = new Role
                    {
                        Id = int.Parse(roleElement.Element("Id")?.Value),
                        Name = roleElement.Element("Name")?.Value,
                        Description = roleElement.Element("Description")?.Value,
                        Permissions = new List<Permission>()
                    };
                    // 加载角色权限
                    var permissionIds = roleElement.Element("Permissions")?.Elements("PermissionId").Select(x => int.Parse(x.Value)).ToList();
                    if (permissionIds != null)
                    {
                        foreach (var permissionId in permissionIds)
                        {
                            var permission = Permissions.FirstOrDefault(p => p.Id == permissionId);
                            if (permission != null)
                            {
                                role.Permissions.Add(permission);
                            }
                        }
                    }
                    Roles.Add(role);
                }
            }
        }
    }


}
