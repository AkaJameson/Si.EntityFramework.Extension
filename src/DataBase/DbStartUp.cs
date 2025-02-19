using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.DataBase.Abstraction;
using Si.EntityFramework.Extension.DataBase.Configuration;
using Si.EntityFramework.Extension.DataBase.Entitys;

namespace Si.EntityFramework.Extension.DataBase
{
    public static class DbStartUp
    {
        /// <summary>
        /// 数据库注入服务
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <param name="ExtensionOptionsAction"></param>
        public static void AddApplicationDbContext<TContext>(this IServiceCollection services,
                                                                 Action<DbContextOptionsBuilder> optionsAction,
                                                                 Action<ExtensionDbOptions> ExtensionOptionsAction = null)
                                                                 where TContext : ApplicationDbContext
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
    }
}
