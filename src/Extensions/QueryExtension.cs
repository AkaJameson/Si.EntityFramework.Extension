using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Extensions;
using System.Linq.Expressions;

namespace Si.EntityFramework.Extension.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }

        public static IQueryable<T> PageBy<T>(
            this IQueryable<T> query,
            int pageIndex,
            int pageSize)
        {
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public static async Task<(List<T> Items, int Total)> ToPagedListAsync<T>(
            this IQueryable<T> query,
            int pageIndex,
            int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query.PageBy(pageIndex, pageSize).ToListAsync();
            return (items, total);
        }
        public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> query, bool condition) where T : class
        {
            return condition ? query.AsNoTracking() : query;
        }
    }
}
