using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.DataBase.Abstraction;
using Si.EntityFramework.Extension.Rbac.Entitys;
using Si.EntityFramework.Extension.UnitofWork.Abstraction;

namespace Si.EntityFramework.Extension.Rbac.Kits
{
    public static class UserExtension
    {
        /// <summary>
        /// 添加角色,需要SaveChanage
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task AddRole(this DbContext context,IUser user,List<Role> roles)
        {
            if(context == null)
                throw new Exception("DbContext is null");
            var roleUser = context.Set<UserRole>();
            foreach(var role in roles)
            {
                roleUser.Add(new UserRole()
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
        }
        /// <summary>
        /// 添加角色,需要SaveChanage
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static async Task AddRole(this IRepository<UserRole> repository, IUser user,List<Role> roles)
        {
            foreach(var role in roles)
            {
                await repository.AddAsync(new UserRole()
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
        }
        /// <summary>
        /// 删除角色,需要SaveChange
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static async Task DeleteRole(this DbContext dbContext,IUser user,List<Role> roles)
        {
            var roleId = roles.Select(p => p.Id).ToList();
            var preDelete = dbContext.Set<UserRole>().Where(p => p.UserId == user.Id && roleId.Contains(p.RoleId));
            dbContext.Set<UserRole>().RemoveRange(preDelete);
        }
        /// <summary>
        /// 删除角色,需要SaveChange
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static async Task DeleteRole(this IRepository<UserRole> repository, IUser user,List<Role> roles)
        {
            var roleId = roles.Select(p => p.Id).ToList();
            var preDelete = repository.Query().Where(p => p.UserId == user.Id && roleId.Contains(p.RoleId));
            await repository.DeleteRangeAsync(preDelete);
        }
        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<List<Role>> GetRoles(this DbContext dbContext,IUser user)
        {
            var roleIds = dbContext.Set<UserRole>().Where(p => p.UserId == user.Id).Select(p => p.RoleId).ToList();
            return await dbContext.Set<Role>().Where(p => roleIds.Contains(p.Id)).ToListAsync();
        }
        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<List<Role>> GetRoles(this IUnitOfWork unitOfWork,IUser user)
        {
            var roleIds = unitOfWork.GetRepository<UserRole>().Query().Where(p => p.UserId == user.Id).Select(p => p.RoleId).ToList();
            return await unitOfWork.GetRepository<Role>().Query().Where(p => roleIds.Contains(p.Id)).ToListAsync();
        }
    }
}
