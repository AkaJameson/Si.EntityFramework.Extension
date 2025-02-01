using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DynamicTableExtensions
{
    private static DynamicTableOptions options { get; set; } = new DynamicTableOptions();

    public static void UseDynamicTable(this IServiceCollection services, DynamicTableOptions options)
    {
        options = options;
    }
    public static IQueryable<T> FromTable<T>(
        this DbContext context,
        object routeValue) where T : class
    {
        var router = new TableRouter(options);
        var tableName = router.GetTableName<T>(routeValue);
        return context.Set<T>().FromSql($"SELECT * FROM {tableName}");
    }

    public static async Task<bool> EnsureTableExistsAsync<T>(
        this DbContext context,
        object routeValue,
        List<TableColumn> columns)
    {
        var router = new TableRouter(options);
        var builder = new TableBuilder(context, options);
        var tableName = router.GetTableName<T>(routeValue);
        await builder.CreateTableAsync(tableName, columns);
        return true;
    }
} 