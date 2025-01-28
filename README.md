# Si.EntityFramework.Extension

一个功能强大的 Entity Framework Core 扩展库,提供工作单元、仓储模式、雪花ID、软删除、审计日志等功能。

## 📦 安装

```bash
dotnet add package Si.EntityFramework.Extension
```

## ✨ 主要功能

- 🏭 工作单元模式
- 📦 通用仓储模式
- ❄️ 雪花ID生成
- 🗑️ 软删除
- 📝 审计日志
- 🏢 多租户
- 📊 性能监控
- 🔄 事务重试
- 💾 JSON字段
- 🔒 并发控制

## 🚀 快速开始

### 1. 创建 DbContext

```csharp
public class YourDbContext : ApplicationDbContext
{
    public YourDbContext(
        DbContextOptions options,
        ExtensionDbOptions extensionOptions,
        ICurrentUser currentUser = null,
        ICurrentTenant currentTenant = null)
        : base(options, extensionOptions, currentUser, currentTenant)
    {
    }
}
```

### 2. 注册服务

```csharp
// 注册 DbContext
builder.Services.AddApplicationDbContext<YourDbContext>(options =>
{
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
}, options =>
{
    options.EnableAudit = true;
    options.EnableSoftDelete = true;
    options.EnableSnowflakeId = true;
    options.EnableMultiTenant = true;
    options.WorkerId = 1;
    options.DatacenterId = 1;
});

// 注册工作单元
builder.Services.AddUnitofWork<YourDbContext>();

// 注册当前用户
builder.Services.AddCurrentUserAccessor(provider => new CurrentUser());

// 注册当前租户
builder.Services.AddCurrentTenantAccessor(provider => new CurrentTenant());
```

## 💡 高级功能

### 仓储模式 API

```csharp
// 1. 基础查询
var repository = _unitOfWork.GetRepository<User>();
var user = await repository.GetByIdAsync(1);
var users = await repository.GetAllAsync();
var activeUsers = await repository.FindAsync(u => u.IsActive);
var admin = await repository.SingleOrDefaultAsync(u => u.Role == "Admin");

// 2. 分页查询
var pagedUsers = await repository.GetPagedListAsync(1, 10);

// 3. 添加实体
await repository.AddAsync(user);
await repository.AddRangeAsync(users);

// 4. 更新实体
await repository.ForceUpdateAsync(user);
await repository.ForceUpdateRangeAsync(users);

// 5. 删除实体
await repository.DeleteAsync(user);
await repository.DeleteRangeAsync(users);
```

### 原生 SQL 查询

```csharp
// 1. 查询返回实体集合
var users = await dbContext.Database.FromSqlCollectionAsync<UserDto>(
    "SELECT * FROM Users WHERE Age > @p0",
    new SqlParameter("@p0", 18)
);

// 2. 查询返回 DataTable
var dt = await dbContext.Database.SqlQueryAsync(
    CommandType.Text,
    "SELECT * FROM Users WHERE DepartmentId = @deptId",
    new SqlParameter("@deptId", 1)
);

// 3. 执行存储过程
var result = await dbContext.Database.SqlQueryAsync(
    CommandType.StoredProcedure,
    "sp_GetUserStats",
    new SqlParameter("@startDate", DateTime.Today)
);
```

### JSON 字段支持

```csharp
// 1. 实体配置
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 配置单个对象为 JSON
        builder.Property(u => u.Settings)
            .HasJsonConversion();
            
        // 配置集合为 JSON
        builder.Property(u => u.Tags)
            .HasJsonConversion();
    }
}

// 2. 实体定义
public class User
{
    public int Id { get; set; }
    public UserSettings Settings { get; set; }
    public List<string> Tags { get; set; }
}
```

### 查询扩展

```csharp
// 1. 条件查询
var query = dbContext.Users
    .WhereIf(age.HasValue, u => u.Age >= age.Value)
    .WhereIf(!string.IsNullOrEmpty(name), u => u.Name.Contains(name));

// 2. 分页查询
var pagedUsers = await query
    .PageBy(pageIndex, pageSize)
    .ToListAsync();

// 3. 带总数的分页查询
var (items, total) = await query
    .ToPagedListAsync(pageIndex, pageSize);

// 4. 禁用跟踪查询
var users = await query
    .AsNoTracking(true)
    .ToListAsync();
```

### 变更追踪

```csharp
// 1. 获取修改的实体
var modifiedEntries = dbContext.ChangeTracker.GetModifiedEntries();

// 2. 获取修改的属性
foreach(var entry in modifiedEntries)
{
    var modifiedProps = entry.GetModifiedProperties();
    
    // 3. 获取属性的新旧值
    var modifiedValues = entry.GetModifiedValues();
    foreach(var (propName, (oldValue, newValue)) in modifiedValues)
    {
        Console.WriteLine($"属性: {propName}");
        Console.WriteLine($"原值: {oldValue}");
        Console.WriteLine($"新值: {newValue}");
    }
}
```

## 📝 注意事项

### JSON 字段
- JSON 字段在数据库中存储为文本类型
- 不支持直接在 JSON 字段上进行数据库级别的查询
- 需要考虑 JSON 字段的大小限制
- 建议为频繁查询的字段创建额外的列而不是放在 JSON 中

### 原生 SQL
- 使用参数化查询避免 SQL 注入
- 注意不同数据库的 SQL 语法差异
- 复杂查询建议使用存储过程
- 需要注意连接的释放

### 仓储模式
- 优先使用仓储接口而不是直接访问 DbContext
- 复杂查询可以扩展仓储接口
- 注意实体间的关联关系
- 批量操作时注意性能

### 其他
- 雪花 ID 需要确保 WorkerId 和 DatacenterId 唯一
- 多租户过滤会自动应用到查询
- 软删除实体默认查询时会过滤已删除记录
- 变更追踪可能会影响性能，按需使用

## 📄 许可证

MIT License

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📚 API 文档

详细的 API 文档请参考源代码注释。