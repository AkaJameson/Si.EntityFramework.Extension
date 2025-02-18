using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Abstraction;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.Entitys;
using Si.EntityFramework.Extension.Rbac.Entitys;
using Si.EntityFramework.Extension.Rbac.Handlers;
using Si.EntityFramework.Extension.Rbac.Kits;
using Si.EntityFramework.Extension.UnitofWork;

namespace Si.EntityFramework.Extension.Extensions
{
    public static class WebApplicationExtension
    {
        public static void AddApplicationDbContext<TContext>(this IServiceCollection services,
             Action<DbContextOptionsBuilder> optionsAction, Action<ExtensionDbOptions> ExtensionOptionsAction = null) where TContext : ApplicationDbContext
        {
            services.AddScoped<IUserInfo, UserInfo>();
            var options = new ExtensionDbOptions();
            ExtensionOptionsAction?.Invoke(options);
            services.AddSingleton(options);
            services.AddDbContext<TContext>(option =>
            {
                optionsAction(option);
            });
        }
        /// <summary>
        /// 添加工作单元
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        public static void AddUnitofWork<TContext>(this IServiceCollection services) where TContext : ApplicationDbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        }
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
            var jwtManager = new TokenManager(option);
            services.AddSingleton(jwtManager);
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
