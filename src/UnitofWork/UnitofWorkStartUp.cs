using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.Database;
using Si.EntityFramework.Extension.UnitofWork.Abstraction;

namespace Si.EntityFramework.Extension.UnitofWork
{
    public static class UnitofWorkStartUp
    {
        /// <summary>
        /// 添加工作单元
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        public static void AddUnitofWork<TContext>(this IServiceCollection services) where TContext : ApplicationDbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        }
    }
}
