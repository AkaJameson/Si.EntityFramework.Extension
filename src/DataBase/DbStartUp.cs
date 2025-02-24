using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.DataBase.Configuration;
using Si.EntityFramework.Extension.DataBase.Kits;
using System.Collections.Concurrent;

namespace Si.EntityFramework.Extension.DataBase
{
    public static class DbStartUp
    {
        internal static ConcurrentDictionary<string, ExDbOptions> ExOptions = new ConcurrentDictionary<string, ExDbOptions>();
        /// <summary>
        /// 数据库注入服务
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <param name="ExOptionsAction"></param>
        public static void AddApplicationDbContext<TContext>(this IServiceCollection services,
                                                                 Action<DbContextOptionsBuilder> optionsAction,
                                                                 Action<ExDbOptions> ExOptionsAction = null)
                                                                 where TContext : ApplicationDbContext 
        {
            var options = new ExDbOptions();
            ExOptionsAction?.Invoke(options);
            ExOptions.TryAdd(typeof(TContext).Name, options);
            services.AddDbContext<TContext>(optionsAction);
        }
        public static void AddApplicationDbContext<TContext>(this IServiceCollection services,
                                                                   Action<DbContextOptionsBuilder> optionsAction, MutiDbOptions mutiDbOptions,
                                                                   Action<ExDbOptions> ExOptionsAction = null)
                                                                   where TContext : ApplicationDbContext
        {
            services.AddSingleton((p) =>
            {
                var router = new DbContextRouter<TContext>(mutiDbOptions);
                return router;
            });
            services.AddScoped<CommandAnalysisInterceptor<TContext>>();
            services.AddScoped<ConnectionSwitchInterceptor<TContext>>();
            var options = new ExDbOptions();
            ExOptionsAction?.Invoke(options);
            ExOptions.TryAdd(typeof(TContext).Name, options);
            services.AddDbContext<TContext>((sp, optionsBuilder) =>
            {
                var interceptors = new List<DbConnectionInterceptor>
                {
                    sp.GetRequiredService<ConnectionSwitchInterceptor<TContext>>(),
                    sp.GetRequiredService<ConnectionSwitchInterceptor<TContext>>()
                };
                optionsBuilder.AddInterceptors(interceptors.ToArray());
                optionsAction(optionsBuilder);
            });
        }
    }
}
