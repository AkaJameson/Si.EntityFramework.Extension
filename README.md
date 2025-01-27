# Si.EntityFramework.Extension

一个功能强大的Entity Framework Core扩展库，提供工作单元模式、仓储模式、雪花ID生成、软删除、审计日志，多租户支持等功能。

## 📦 安装

```bash
dotnet add package Si.EntityFramework.Extension
```

## ✨ 主要功能

- 🏭 工作单元（UnitOfWork）模式
- 📦 通用仓储模式
- ❄️ 雪花ID生成器
- 🗑️ 软删除支持
- 📝 审计日志
- 🏢 多租户支持
- 📊 性能监控
- 🔄 事务重试机制
- 💾 JSON字段支持
- 🔒 并发控制

## 🚀 快速开始

### 1. 创建DbContext

```csharp
public class YourDbContext : SiDbContext
{
    public YourDbContext(
        DbContextOptions<YourDbContext> options, 
        IOptions<SiDbContextOptions> siOptions,
        ICurrentUser currentUser = null,
        ICurrentTenant currentTenant = null) 
        : base(options, siOptions.Value, currentUser, currentTenant)
    {
    }
}
```

### 2. 注册服务

```csharp
// 注册DbContext和扩展功能
builder.Services.AddSiDbContext<YourDbContext>(options =>
{
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
}, siOptions =>
{
    siOptions.EnableAudit = true;
    siOptions.EnableSoftDelete = true;
    siOptions.EnableSnowflakeId = true;
    siOptions.EnableMultiTenant = true;
    siOptions.DatacenterId = 1;
    siOptions.WorkerId = 1;
});

// 注册工作单元
builder.Services.AddUnitofWork<YourDbContext>();

// 注册当前用户访问器（如果启用审计功能）
builder.Services.AddCurrentUserAccessor(provider =>
{
    // 实现获取当前用户的逻辑
    return new CurrentUser();
});

// 注册当前租户访问器（如果启用多租户）
builder.Services.AddCurrentTenantAccessor(provider =>
{
    // 实现获取当前租户的逻辑
    return new CurrentTenant();
});
```

### 3. 实体配置

#### 雪花ID实体
```csharp
public class User : ISnowflakeId
{
    public long Id { get; set; }
    public string Name { get; set; }
}
```

#### 软删除实体
```csharp
public class Product : ISoftDelete
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedTime { get; set; }
}
```

#### 审计实体
```csharp
public class Order : AuditedEntityBase
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
}
```

#### 多租户实体
```csharp
public class Customer : IMultiTenant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TenantId { get; set; }
}
```

### 4. 使用工作单元和仓储

```csharp
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
        
        await _unitOfWork.ExecuteTransactionWithRetryAsync(async () =>
        {
            await repository.AddAsync(user);
            await _unitOfWork.CommitAsync();
        }, retryCount: 3);
    }
}
```

## 💡 高级功能

### JSON字段支持

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Settings)
            .HasJsonConversion();
            
        builder.Property(u => u.Tags)
            .HasJsonConversion();
    }
}
```

### 并发控制

```csharp
// 悲观锁
var user = await dbContext.GetWithLockAsync<User>(userId);

// 乐观锁
var success = await dbContext.TryOptimisticUpdateAsync(user, entity => 
{
    entity.Name = "新名字";
});
```

### 性能监控

```csharp
services.AddDbContext<YourDbContext>((sp, options) => 
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(new QueryPerformanceInterceptor());
});
```

### 直接SQL查询

```csharp
var results = await dbContext.Database.FromSqlCollectionAsync<UserDto>(
    "SELECT * FROM Users WHERE Age > @p0",
    new SqlParameter("@p0", 18)
);
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

### 其他注意事项

- 雪花ID生成器需要确保WorkerId和DatacenterId在分布式环境中的唯一性
- 使用仓储级别保存时需要注意实体间的关联关系
- 性能监控可能会对性能产生轻微影响，建议在开发环境中使用

## 📄 许可证

MIT License

Copyright (c) 2025 Simon Jameson

此软件及相关文档文件（以下简称"软件"）在遵循以下条件的情况下，免费提供给任何人：

1. 允许在软件的副本中使用、复制、修改、合并、发布、分发、再授权和/或出售该软件的副本，但必须满足以下条件：
   
   - 在所有软件的副本或主要部分中都包含上述版权声明和本许可声明。

2. 本软件是按"原样"提供的，不附带任何形式的明示或暗示的担保，包括但不限于适销性、特定用途的适用性以及非侵权的保证。在任何情况下，作者或版权持有人都不对因软件的使用或其他交易行为而产生的任何索赔、损害或其他责任承担责任，无论是合同、侵权行为还是其他方式。

## 🤝 贡献

欢迎提交Issue和Pull Request！

## 📚 API文档

详细的API文档请参考源代码注释。
