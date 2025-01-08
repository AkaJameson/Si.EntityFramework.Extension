# Si.EntityFramework.Extension

一个轻量级的 Entity Framework Core 扩展库，提供了工作单元模式、仓储模式、雪花ID生成、性能监控等功能。

## ✨ 特性

🏭 工作单元（UnitOfWork）模式，事务级别存储

📦 通用仓储模式

❄️ 雪花ID生成器

📊 查询性能监控

🔍 变更追踪

📝 SQL执行扩展

🎯 支持仓储级别的保存

## 📦 安装

```
dotnet add package Si.Framework.EntityFramework
```

## 🚀 快速开始

### 1. 配置 DbContext

```c#
public class YourDbContext : SiDbContextBase
{
    public YourDbContext(DbContextOptions options, IdGenerator idGenerator = null) 
        : base(options, idGenerator)
    {
    }
}
```

### 2. 注册服务

```c#
services.AddScoped<IUnitOfWork, UnitOfWork<YourDbContext>>();
services.AddSingleton<IdGenerator>(new IdGenerator(1, 1)); // workerId, datacenterId
services.AddDbContext<YourDbContext>(options => 
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
```

### 3. 使用工作单元和仓储

```c#
public class UserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateUserAsync(User user)
    {
        var repository = _unitOfWork.GetRepository<User>();
        await repository.AddAsync(user);
        await _unitOfWork.CommitAsync();
    }

    public asyn Task CreateUserAsync(User user,UserRole role)
    {
         var repository = _unitOfWork.GetRepository<User>();
        _unitofWork.ExecuteTransactionAsync(()=>
        {
            //事务内工作
            repository.SaveRepository();
        });
        //保存
        await _unitofWork.CommitAsync();
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



### 批量操作

```c#
await repository.AddRangeAsync(entities);
await repository.UpdateRangeAsync(entities);
await repository.DeleteRangeAsync(entities);
```

## 📝 注意事项

雪花ID生成器需要确保 workerId 和 datacenterId 在分布式环境中的唯一性

使用仓储级别保存时需要注意实体间的关联关系

性能监控可能会对性能产生轻微影响，建议在开发环境中使用

## 🤝 贡献

欢迎提交问题和改进建议！
