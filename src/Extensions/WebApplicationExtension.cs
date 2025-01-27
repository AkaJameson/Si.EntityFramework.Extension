using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Abstraction;
using Si.EntityFramework.Extension.DataBase;
using Si.EntityFramework.Extension.Entitys;
using Si.EntityFramework.Extension.UnitofWork;

namespace Si.EntityFramework.Extension
{
    public static class WebApplicationExtension
    {
        public static void AddSiDbContext<TContext>(this IServiceCollection services,
             Action<DbContextOptionsBuilder> optionsAction, Action<SiDbContextOptions> ExtensionOptionsAction = null) where TContext : SiDbContext
        {
            var options = new SiDbContextOptions();
            ExtensionOptionsAction?.Invoke(options);
            services.AddSingleton(options);
            services.AddDbContext<TContext>(option =>
            {
                optionsAction(option);
            });
        }
        /// <summary>
        /// 添加当前用户访问器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="CurrentUserFactory"></param>
        public static void AddCurrentUserAccessor(this IServiceCollection services, Func<IServiceProvider, ICurrentUser> CurrentUserFactory)
        {
            services.AddScoped<ICurrentUser>(CurrentUserFactory);
        }
        /// <summary>
        /// 添加工作单元
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        public static void AddUnitofWork<TContext>(this IServiceCollection services)where TContext : SiDbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        }
        /// <summary>
        /// 添加当前租户访问器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="CurrentTenant"></param>
        public static void AddCurrentTenantAccessor(this IServiceCollection services,Func<IServiceProvider,ICurrentTenant> CurrentTenant)
        {
            services.AddScoped<ICurrentTenant>(CurrentTenant);
        }
    }
}
