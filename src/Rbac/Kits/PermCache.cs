using Si.EntityFramework.PermGuard.Entitys;
using System.Collections.Concurrent;

namespace Si.EntityFramework.PermGuard.Kits
{
    public class PermCache
    {
        /// <summary>
        /// 角色缓存
        /// </summary>
        internal static ConcurrentDictionary<string, Role> _roleCache = new ConcurrentDictionary<string, Role>();
        /// <summary>
        /// 刷新角色缓存
        /// </summary>
        /// <param name="roles"></param>
        public static void RefreshRoleCache(IEnumerable<Role> roles)
        {
            _roleCache.Clear();
            foreach (var item in roles)
            {
                _roleCache[item.Name] = item;
            }
        }
        /// <summary>
        /// 检查给定的角色列表中是否包含指定的权限。
        /// </summary>
        /// <param name="roleName">角色名称列表，用于检查权限。</param>
        /// <param name="permissionName">需要检查的权限名称。</param>
        /// <returns>如果存在指定的权限，则返回true；否则返回false。</returns>
        internal static bool HasPermission(List<string> roleName, string permissionName)
        {
            foreach (var role in roleName ?? new List<string>())
            {
                if (_roleCache.ContainsKey(role) && _roleCache[role].Permissions.Any(p => p.Name == permissionName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
