﻿using Si.EntityFramework.Extension.Rbac.Entitys;
using System.Collections.Concurrent;
using System.Data;

namespace Si.EntityFramework.Extension.Rbac.Kits
{
    public class PermCache
    {
        /// <summary>
        /// 角色缓存 (RoleName,HashSet(PermissionName)
        /// </summary>
        internal static ConcurrentDictionary<string, HashSet<string>> _roleCache = new ConcurrentDictionary<string, HashSet<string>>();
        /// <summary>
        /// 刷新角色缓存
        /// </summary>
        /// <param name="roles"></param>
        public static void RefreshRoleCache(IEnumerable<Role> roles)
        {
            _roleCache.Clear();
            foreach (var item in roles)
            {
                var permissions = item.Permissions.Select(p => p.Name).ToHashSet();
                _roleCache[item.Name] = permissions;
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
                if (_roleCache.ContainsKey(role) && _roleCache[role].Any(p => p.Contains(permissionName)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
