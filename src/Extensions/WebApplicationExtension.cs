﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.DataBase;
using Si.EntityFramework.Extension.Entitys;

namespace Si.EntityFramework.Extension
{
    public static class WebApplicationExtension
    {
        public static void AddSiDbContext<TContext>(this IServiceCollection services,
             Action<DbContextOptionsBuilder> optionsAction, Action<SiDbContextOptions> ExtensionOptionsAction = null) where TContext : SiDbContext
        {
            var options = new SiDbContextOptions();
            if (ExtensionOptionsAction != null)
            {
                ExtensionOptionsAction(options);
            }
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
    }
}
