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
        public static void AddApplicationDbContext<TContext,TExtensionOption>(this IServiceCollection services,
                                                                 Action<DbContextOptionsBuilder> optionsAction,
                                                                 Action<TExtensionOption> ExtensionOptionsAction = null)
                                                                 where TContext : ApplicationDbContext where TExtensionOption : ExtensionDbOptions,new()
        {
            services.AddScoped<IUserInfo, UserInfo>();
            services.AddScoped((sp) =>
            {
                return new ExtensionDbOptions();
            });
            var options = new TExtensionOption();
            ExtensionOptionsAction?.Invoke(options);
            services.AddSingleton(options);
            services.AddDbContext<TContext>((sp, optionsBuilder) =>
            {
                var extensionDbOptions = sp.GetRequiredService<ExtensionDbOptions>();
                extensionDbOptions = sp.GetRequiredService<TExtensionOption>();
                optionsAction(optionsBuilder);
            });
        }
    }
}
