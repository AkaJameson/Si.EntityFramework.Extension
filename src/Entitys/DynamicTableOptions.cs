public class DynamicTableOptions
{
    /// <summary>
    /// 是否启用动态表
    /// </summary>
    public bool EnableDynamicTable { get; set; } = false;

    /// <summary>
    /// 表名前缀
    /// </summary>
    public string TablePrefix { get; set; } = string.Empty;

    /// <summary>
    /// 分表策略
    /// </summary>
    public TableShardingStrategy ShardingStrategy { get; set; } = TableShardingStrategy.None;

    /// <summary>
    /// 自定义路由规则
    /// </summary>
    public Func<object, string> CustomRouteRule { get; set; }
}

public enum TableShardingStrategy
{
    None,
    ByMonth,
    ByYear,
    ByDay,
    Custom
} 