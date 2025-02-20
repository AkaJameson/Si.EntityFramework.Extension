using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.DataBase.Abstraction;
using Si.EntityFramework.Extension.DataBase.Configuration;
using Si.EntityFramework.Extension.DataBase.Entitys;
using Si.EntityFramework.Extension.MultiDbContext.Configuration;
using Si.EntityFramework.Extension.MultiDbContext.Kits;
using System;

namespace Si.EntityFramework.Extension.MultiDbContext
{
    public static class MuliDbStartUp
    {
        public static void AddApplicationDbContext<TContext, TExtensionOption>(this IServiceCollection services,
                                                                   Action<DbContextOptionsBuilder> optionsAction, MutiDbOptions mutiDbOptions,
                                                                   Action<TExtensionOption> ExtensionOptionsAction = null)
                                                                   where TContext : ApplicationDbContext where TExtensionOption : ExtensionDbOptions, new()
        {
            services.AddScoped<DbContextRouter<TContext>>((p) =>
            {
                var router = new DbContextRouter<TContext>(mutiDbOptions);
                return router;
            });
            services.AddScoped<CommandAnalysisInterceptor<TContext>>();
            services.AddScoped<ConnectionSwitchInterceptor<TContext>>();
            services.AddScoped<IUserInfo, UserInfo>();
            services.AddScoped<ExtensionDbOptions>();
            var options = new TExtensionOption();
            ExtensionOptionsAction?.Invoke(options);
            services.AddSingleton(options);
            services.AddDbContext<TContext>((sp, optionsBuilder) =>
            {
                var extensionDbOptions = sp.GetRequiredService<ExtensionDbOptions>();
                extensionDbOptions = sp.GetRequiredService<TExtensionOption>();
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
