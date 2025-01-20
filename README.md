# Si.EntityFramework.Extension

一个基于 Entity Framework Core 的扩展库，提供工作单元模式、仓储模式、雪花ID生成、软删除、审计日志、性能监控等功能。

## ✨ 特性

🏭 工作单元（UnitOfWork）模式，事务级别存储

📦 通用仓储模式

❄️ 雪花ID生成器

📊 查询性能监控

🔍 变更追踪

📝 SQL执行扩展

🎯 支持仓储级别的保存

🗑️ 软删除支持

📝 审计日志跟踪

⚙️ 功能可配置

🔁 事务重试机制

## 📦 安装

```
paket add Si.EntityFramework.Extension --version xxxxx
```

## 🚀 快速开始

###1.创建dbContext

```c#
public class YourDbContext : SiDbContextBase
{
	public YourDbContext(DbContextOptions<YourDbContext> options,IOptions<SiDbContextOptions> siOptions,ICurrentUser currentUser = null):base(options, siOptions.Value, currentUser)
	{
	}
}
```

### 2. 注册服务

```c#
//注册DbContext
builder.Services.AddSiDbContext<YourDbContext>(option =>
 {
     option.UseSqlite("Data Source=mydatabase.db");
 }, ExtensionOptions =>
    {
        // 启用审计日志
        ExtensionOptions.EnableAudit = true;
        // 启用软删除
        ExtensionOptions.EnableSoftDelete = true;
        // 启用雪花ID
        ExtensionOptions.EnableSnowflakeId = true;
        // 设置数据中心ID和机器ID
        ExtensionOptions.DatacenterId = 1;
        ExtensionOptions.WorkerId = 1;
    });
// 如果启用审计功能，需要注册当前用户服务
 builder.Services.AddCurrentUserAccessor(provider =>
 {
     // 自定义获取当前用户的方法
 });
//启用工作单元
services.AddScoped<IUnitOfWork, UnitOfWork<YourDbContext>>();
```

### 4. 使用工作单元和仓储

```c#
public class UserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
	// 普通事务
	await unitOfWork.ExecuteTransactionAsync(async () =>
	{
		await repository.AddAsync(entity);
		await unitOfWork.CommitAsync();
	});
	// 带重试的事务
	await unitOfWork.ExecuteTransactionWithRetryAsync(async () =>
    {
    	await repository.AddAsync(entity);
		await unitOfWork.CommitAsync();
    }, retryCount: 3);
}
```

## 💡 主要功能

### 雪花ID生成

```c#
public class User : ISnowflakeId
{
    public long Id { get; set; }
    public string Name { get; set; }
}

```

### 软删除

```c#
public interface ISoftDelete
{
	bool IsDeleted { get; set; }
	DateTime? DeletedTime { get; set; }
}
```

```c#
// 软删除
await repository.SoftDeleteAsync(entity);
// 恢复删除
await repository.RestoreAsync(entity);
// 查询包含已删除的数据
var allData = repository.GetAllIncludeDeleted();
```

### 审计功能

提供三个级别的审计接口:

```
public interface ICreationAudited
{
	string CreatedBy { get; set; }
	DateTime CreatedTime { get; set; }
}
public interface IModificationAudited
{
	string LastModifiedBy { get; set; }
	DateTime? LastModifiedTime { get; set; }
}
public interface IFullAudited : ICreationAudited, IModificationAudited, ISoftDelete
{
	string DeletedBy { get; set; }
}
```



### 查询性能监控

```c#
services.AddDbContext<YourDbContext>((sp, options) => 
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(new QueryPerformanceInterceptor());
});
```

### 仓储级别保存

```c#
var repository = _unitOfWork.GetRepository<User>();
await repository.AddAsync(user);
// 只保存User相关的更改
await repository.SaveRepository();
```

### SQL直接查询

```c#
var result = await dbContext.Database.FromSqlCollectionAsync<UserDto>(
    "SELECT * FROM Users WHERE Age > @p0",
    new SqlParameter("@p0", 18)
);
```

### JSON字段支持

支持将复杂对象或集合以JSON格式存储在数据库中：

```csharp
// 实体配置
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 配置复杂类型属性为JSON存储
        builder.Property(u => u.Address)
            .HasJsonConversion();
            
        // 配置集合类型为JSON存储
        builder.Property(u => u.Tags)
            .HasJsonConversion();
    }
}

// 使用自定义JSON选项
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

builder.Property(u => u.Address)
    .HasJsonConversion(jsonOptions);
```

### 查询扩展

提供多个便捷的查询扩展方法：

```csharp
// 条件查询
var query = dbContext.Users
    .WhereIf(!string.IsNullOrEmpty(name), u => u.Name.Contains(name))
    .WhereIf(age > 0, u => u.Age > age);
// 分页查询
var (items, total) = await query.ToPagedListAsync(pageIndex, pageSize);
// 无跟踪查询
var users = query.AsNoTracking(condition: true);
```

### JSON序列化选项

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true // 格式化JSON
};

// 在实体配置中使用
builder.Property(u => u.ComplexData)
    .HasJsonConversion(options);
```

### 分页查询

```csharp
// 基础分页
var pagedData = await query.PageBy(pageIndex, pageSize).ToListAsync();

// 带总数的分页
var (items, total) = await query.ToPagedListAsync(pageIndex, pageSize);
```

## 📚 API文档

### IUnitOfWork

```
public interface IUnitOfWork
{
    IRepository<T> GetRepository<T>() where T : class;
    Task<int> CommitAsync();
    Task ExecuteTransactionAsync(Func<Task> action);
    Task ExecuteTransactionWithRetryAsync(Func<Task> action, int retryCount = 3);
    void Rollback();
}
```

### IRepository<T>

```c#
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    // ... 更多方法
}
```

## 🔧 高级功能

### 变更追踪

```c#
var changes = entry.GetModifiedValues();
foreach (var (property, (oldValue, newValue)) in changes)
{
    Console.WriteLine($"{property}: {oldValue} -> {newValue}");
}
```

### 无跟踪查询

```c#
// 使用无跟踪查询提升性能
var users = dbContext.ReadQuery<User>().Where(u => u.Age > 18);
```

### 批量操作

```c#
await repository.AddRangeAsync(entities);
await repository.UpdateRangeAsync(entities);
await repository.DeleteRangeAsync(entities);
```

### 并发控制

提供悲观锁和乐观锁机制：

```csharp
// 悲观锁
var user = await dbContext.GetWithLockAsync<User>(userId);
user.Name = "新名字";
await dbContext.SaveChangesAsync();

// 乐观锁
var success = await dbContext.TryOptimisticUpdateAsync(user, entity => 
{
    entity.Name = "新名字";
});
```

支持多种数据库的锁实现：
- SQL Server: `WITH (UPDLOCK, ROWLOCK)`
- PostgreSQL: `FOR UPDATE`
- MySQL/MariaDB: `FOR UPDATE`

### 并发控制策略

```csharp
// 带重试的悲观锁
public async Task UpdateWithRetryAsync(long id)
{
    for (int i = 0; i < 3; i++)
    {
        try
        {
            var entity = await _dbContext.GetWithLockAsync<User>(id);
            // 更新操作
            await _dbContext.SaveChangesAsync();
            break;
        }
        catch (Exception) when (i < 2)
        {
            await Task.Delay(100 * (i + 1));
        }
    }
}
```

## 📝 注意事项

### JSON字段注意事项

- JSON字段在数据库中存储为文本类型
- 不支持直接在JSON字段上进行数据库级别的查询
- 需要考虑JSON字段的大小限制
- 建议为频繁查询的字段创建额外的列而不是放在JSON中

### 并发控制注意事项

- 悲观锁会降低并发性能，建议仅在必要时使用
- 不同数据库的锁实现可能略有差异
- 使用乐观锁时需要处理更新失败的情况
- 建议配合重试机制使用

雪花ID生成器需要确保 workerId 和 datacenterId 在分布式环境中的唯一性

使用仓储级别保存时需要注意实体间的关联关系

性能监控可能会对性能产生轻微影响，建议在开发环境中使用
