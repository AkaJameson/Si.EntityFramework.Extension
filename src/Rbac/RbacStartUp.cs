using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.Rbac.Configuration;
using Si.EntityFramework.Extension.Rbac.Handlers;
using Si.EntityFramework.Extension.Rbac.Kits;

namespace Si.EntityFramework.Extension.Rbac
{
    public static class RbacStartUp
    {
        /// <summary>
        /// 向服务集合中添加RBAC核心服务配置。
        /// </summary>
        /// <param name="services">IServiceCollection实例，用于注册服务。</param>
        /// <param name="configure">配置RbacOptions的委托。</param>
        public static void AddRbacCore(this IServiceCollection services, Action<RbacOptions> configure)
        {
            var option = new RbacOptions();
            configure(option);
            services.AddSingleton(option);
            var tokenManager = new TokenManager(option);
            services.AddSingleton(tokenManager);
        }
        /// <summary>
        /// 在Web应用程序中使用RBAC核心功能。
        /// </summary>
        /// <param name="app">WebApplication实例，用于配置中间件。</param>
        /// <typeparam name="DbContext">继承自ApplicationDbContext的数据库上下文类型。</param>
        public static void UseRbacCore<DbContext>(this WebApplication app) where DbContext : ApplicationDbContext
        {
            var option = app.Services.GetRequiredService<RbacOptions>();
            using var sc = app.Services.CreateScope();
            var _context = sc.ServiceProvider.GetRequiredService<DbContext>();
            var permInitializer = new PermInitializer<DbContext>(_context, option);
            permInitializer.Initialize();
            app.UseMiddleware<AuthorizationMiddleware>();
        }
        /// <summary>
        /// 用户信息查询
        /// </summary>
        /// <param name="app"></param>
        public static void UseInfoParser(this WebApplication app)
        {
            app.UseMiddleware<UserInfoMiddleware>();
        }
    }
}
