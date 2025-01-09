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
dotnet add package Si.Framework.EntityFramework
```

## 🚀 快速开始

### 1. 参数配置

```c#
services.Configure<SiDbContextOptions>(options =>
{
options.EnableSnowflakeId = true; // 启用雪花ID
options.EnableSoftDelete = true; // 启用软删除
options.EnableAudit = true; // 启用审计
options.WorkerId = 1; // 雪花ID的WorkerId
options.DatacenterId = 1; // 雪花ID的DatacenterId
});
// 如果启用审计功能，需要注册当前用户服务
services.AddScoped<ICurrentUser, YourCurrentUserImplementation>();
```

###2.创建dbContext

```c#
public class YourDbContext : SiDbContextBase
{
	public YourDbContext(DbContextOptions<YourDbContext> options,IOptions<SiDbContextOptions> siOptions,ICurrentUser currentUser = null): 		base(options, siOptions.Value, currentUser)
	{
	
	}
}
```

### 3. 注册服务

```c#
services.AddScoped<IUnitOfWork, UnitOfWork<YourDbContext>>();
services.AddDbContext<YourDbContext>(options => 
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
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

### 功能开关配置

```
可以通过 SiDbContextOptions 灵活配置功能:
```

## 📝 注意事项

雪花ID生成器需要确保 workerId 和 datacenterId 在分布式环境中的唯一性

使用仓储级别保存时需要注意实体间的关联关系

性能监控可能会对性能产生轻微影响，建议在开发环境中使用
