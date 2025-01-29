using System;
using System.Collections.Concurrent;

public class TableRouter
{
    private readonly DynamicTableOptions _options;
    private readonly ConcurrentDictionary<string, string> _tableCache = new();

    public TableRouter(DynamicTableOptions options)
    {
        _options = options;
    }

    public string GetTableName<T>(object routeValue)
    {
        var baseTableName = typeof(T).Name;
        
        if (!_options.EnableDynamicTable)
            return baseTableName;

        var suffix = _options.ShardingStrategy switch
        {
            TableShardingStrategy.ByMonth => DateTime.Now.ToString("yyyyMM"),
            TableShardingStrategy.ByYear => DateTime.Now.Year.ToString(),
            TableShardingStrategy.ByDay => DateTime.Now.ToString("yyyyMMdd"),
            TableShardingStrategy.Custom => _options.CustomRouteRule?.Invoke(routeValue)?.ToString(),
            _ => string.Empty
        };

        var tableName = string.IsNullOrEmpty(suffix) 
            ? baseTableName 
            : $"{_options.TablePrefix}{baseTableName}_{suffix}";

        return _tableCache.GetOrAdd(tableName, tableName);
    }
} 