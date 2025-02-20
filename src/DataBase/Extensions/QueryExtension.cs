using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.DataBase.Extensions;
using System.Linq.Expressions;

namespace Si.EntityFramework.Extension.DataBase.Extensions
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
            int pageIndex = 1,
            int pageSize = 20)
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

        public static IQueryable<T> DistinctBy<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> keySelector)
        {
            return source.GroupBy(keySelector)
                         .Select(group => group.First());
        }
        public static void ForEach<T>(this IQueryable<T> source, Action<T> action)
        {
            foreach (var item in source.ToList())  // 强制执行查询
            {
                action(item);
            }
        }
        public static IQueryable<T> Batch<T>(this IQueryable<T> source, int batchSize, int pageIndex)
        {
            return source.Skip(batchSize * (pageIndex - 1))
                         .Take(batchSize);
        }
        public static IQueryable<T> When<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }
    }
}
