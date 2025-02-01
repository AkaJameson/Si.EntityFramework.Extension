using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.DataBase;
using Si.EntityFramework.Extension.Rbac.Entitys;

namespace Si.EntityFramework.Extension.Rbac.Kits
{
    /// <summary>
    /// 权限初始化器
    /// </summary>
    public class PermInitializer<dbContext> where dbContext : ApplicationDbContext
    {
        private readonly dbContext _context;
        private readonly RbacOptions _options;
        public PermInitializer(dbContext context, RbacOptions options)
        {
            _context = context;
            _options = options;
            // 加载权限配置
            options.LoadFromXml();
        }
        public void Initialize()
        {
            // 如果数据库中已经有权限表，则不再初始化
            if (_context.Set<Permission>().Any() || _context.Set<Role>().Any())
            {
                var cacheList = _context.Set<Role>()
                .Include(r => r.Permissions)  // 显式加载权限
                .ToList();

                foreach (var role in cacheList)
                {
                    PermCache._roleCache.TryAdd(role.Name, role.Permissions.Select(p => p.Name).ToHashSet());
                }
            }
            else
            {
                _context.Set<Permission>().AddRange(_options.Permissions);
                _context.Set<Role>().AddRange(_options.Roles);
                _context.SaveChanges();

                var cacheList = _context.Set<Role>()
                    .Include(r => r.Permissions)  // 显式加载权限
                    .ToList();

                foreach (var role in cacheList)
                {
                    PermCache._roleCache.TryAdd(role.Name, role.Permissions.Select(p => p.Name).ToHashSet());
                }
            }
        }
    }
}
