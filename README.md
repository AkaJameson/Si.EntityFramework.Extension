# Si.EntityFramework.Extension

一个功能强大的 Entity Framework Core 扩展库,提供工作单元、仓储模式、雪花ID、软删除、审计日志、RBAC权限控制、多租户等功能。

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
- 📱 RBAC 权限控制

## 🚀 快速开始

### 1. 创建 DbContext

```csharp
public class YourDbContext : ApplicationDbContext
{
    public YourDbContext(DbContextOptions options, ExtensionDbOptions optionsExtension, IUserInfo sessions = null) : base(options, optionsExtension, sessions)
  {
  }
}
```

### 2. 注册服务

```csharp
// 注册 DbContext
 builder.Services.AddApplicationDbContext<YourDbContext>(optionAction =>
 {
     optionAction.UseMySql(connectionStr, ServerVersion.AutoDetect(connectionStr));
     //懒加载
     optionAction.UseLazyLoadingProxies();
 }, ExtensionOptionsActio =>
 {
     //不启动雪花
     ExtensionOptionsActio.EnableSnowflakeId = false;
     //启动审计
     ExtensionOptionsActio.EnableAudit = true;
     //启动软删除
     ExtensionOptionsActio.EnableSoftDelete = true;
     //不启用多租户
     ExtensionOptionsActio.EnableMultiTenant = false;
 });

// 注册工作单元
builder.Services.AddUnitofWork<YourDbContext>();
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

## 🔐 RBAC 权限控制

### 1. 配置权限和角色

创建 `rbac.xml` 配置文件:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Rbac>
    <Permissions>
        <Permission>
            <Id>1</Id>
            <Name>user.read</Name>
            <Description>查看用户权限</Description>
        </Permission>
        <Permission>
            <Id>2</Id>
            <Name>user.write</Name>
            <Description>编辑用户权限</Description>
        </Permission>
    </Permissions>
    <Roles>
        <Role>
            <Id>1</Id>
            <Name>Admin</Name>
            <Description>管理员</Description>
            <Permissions>
                <PermissionId>1</PermissionId>
                <PermissionId>2</PermissionId>
            </Permissions>
        </Role>
        <Role>
            <Id>2</Id>
            <Name>User</Name>
            <Description>普通用户</Description>
            <Permissions>
                <PermissionId>1</PermissionId>
            </Permissions>
        </Role>
    </Roles>
</Rbac>
```

### 2. 注册服务

```csharp
// 注册 RBAC 服务
builder.Services.AddRbacCore(options =>
{
    options.ConfigPath = "rbac.xml";  // 配置文件路径
    options.SecrectKey = "your-secret-key"; // JWT密钥
    options.Issuer = "your-issuer";  // JWT颁发者
    options.Audience = "your-audience"; // JWT接收者
});
.....

//用户信息解析器（必须在Routing之前）配合权限验证中间件进行使用
app.UseInfoParser();
app.UseRouting();
//添加权限验证中间件
app.UseRbacCore<BlogDbContext>();
```

### 3. 权限注解

```csharp
// 控制器或方法级别的权限控制
[Permission("user.read")]
public class UserController : ControllerBase
{
    [Permission("user.write")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        // 实现创建用户的逻辑
    }
    
    [AllowAnonymous] // 允许匿名访问
    public async Task<IActionResult> GetPublicInfo()
    {
        // 实现获取公开信息的逻辑
    }
}
```

### 4. JWT 令牌生成

```csharp
public class AuthService
{
    private readonly JwtManager _jwtManager;
    
    public AuthService(JwtManager jwtManager)
    {
        _jwtManager = jwtManager;
    }
    
    public string GenerateToken(User user)
    {
        return _jwtManager.GenerateToken(
            user.Id,
            user.UserName,
            user.Roles.Select(r => r.Name).ToList(),
            user.tenantId
        );
    }
}
```

### 5. 数据模型

```csharp
// 用户实体
public interface IUser
{
    public int Id { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
}

// 角色实体
public abstract class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<Permission> Permissions { get; set; }
    public virtual ICollection<UserBase> Users { get; set; }
}
//用户角色关联表
public class UserRole
{
    public long UserId { get; set;}
    public int RoleId { get; set; }
}
public class RoleUserConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });
    }
}
// 权限实体
public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
}
/// <summary>
/// 继承
/// </summary>
public class User : IUser
{
    public long Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Blog> Blogs { get; set; }
    public virtual ICollection<Essay> Essays { get; set; }
}
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.Property(x => x.Account).HasMaxLength(50);
        builder.Property(x => x.PasswordHash).HasMaxLength(256);
        builder.HasMany(x => x.Essays).WithOne(x => x.User).HasForeignKey(p => p.UserId);
        builder.HasMany(x => x.Blogs).WithOne(x => x.User).HasForeignKey(x => x.UserId);
    }
}


*注：* 因为考虑到用户扩展的原因并未提供User的导航属性对应，请参考UserExtension的类实现这种关系

## Model配置
protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RoleUserConfiguration()); 
    }

```

## 🔑 RBAC 特性

- 基于 JWT 的身份验证
- 基于角色的权限控制
- XML 配置权限和角色
- 权限缓存机制
- 支持方法和控制器级别的权限控制
- 支持匿名访问
- 支持多角色
- 支持角色继承（通过权限组合）

## ⚠️ RBAC 注意事项

### 配置文件
- XML 文件需要放在正确的路径
- 权限和角色的 ID 必须唯一
- 角色引用的权限 ID 必须存在

### 安全性
- JWT 密钥要足够复杂且需要妥善保管
- 建议使用 HTTPS
- Token 过期时间要合理设置
- 敏感操作建议添加额外验证

### 性能
- 权限缓存机制可以提高验证效率
- 避免过于频繁的角色权限变更
- 合理设计权限粒度

### 扩展性
- 可以通过继承 `PermissionAttribute` 实现自定义权限验证
- 可以通过修改 `JwtManager` 实现自定义令牌生成逻辑
- 可以通过修改中间件实现自定义验证逻辑

## 📚 相关文档

- [JWT 官方文档](https://jwt.io/)
- [ASP.NET Core 授权文档](https://docs.microsoft.com/aspnet/core/security/authorization/introduction)

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

