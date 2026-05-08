# 舌尖账本 DDD 架构通俗解读

> 写给开发者看的"大白话"架构说明文档

---

## 一、为什么要分层？

### 1.1 原始写法的问题

如果你直接把所有代码写在一个文件里，会变成这样：

```
用户请求 → 验证手机号格式 → 生成验证码 → 发送短信 → 存储用户 → 返回Token → 数据库操作
```

所有逻辑搅在一起，像一锅粥。问题是：
- 改一个地方可能影响其他功能
- 很难找到具体代码在哪里
- 无法单独测试某个功能
- 多人协作时容易冲突

### 1.2 分层的好处

把代码按照职责分成不同"楼层"：

```
         ┌─────────────────────────────────────┐
         │           第四层：API层                          │  ← 接待员，只负责接收请求
         └─────────────────────────────────────┘
                        ↓ 调用
         ┌─────────────────────────────────────┐
         │           第三层：应用层                         │  ← 业务逻辑在这里
         └─────────────────────────────────────┘
                        ↓ 调用
         ┌─────────────────────────────────────┐
         │           第二层：领域层                         │  ← 实体和规矩定义
         └─────────────────────────────────────┘
                        ↓ 调用
         ┌─────────────────────────────────────┐
         │           第一层：基础设施层                  │  ← 和数据库、文件等打交道
         └─────────────────────────────────────┘
```

**每一层只知道自己"该知道"的事情，不越界。**

---

## 二、项目结构图

```
src/Backend/
├── ChimeBackend.Api/              ← 第四层：API层（Controllers + Request DTOs）
├── ChimeBackend.Application/      ← 第三层：应用层（AppServices + Result DTOs）
│   ├── DTOs/                   ← 数据传输对象（可被Api层直接引用）
│   │   ├── AuthDTOs.cs
│   │   ├── FoodDTOs.cs
│   │   ├── FoodRecordDTOs.cs
│   │   └── UserDTOs.cs
│   └── Services/                ← 业务逻辑服务
│       ├── AuthAppService.cs
│       ├── FoodAppService.cs
│       ├── FoodRecordAppService.cs
│       └── UserAppService.cs
├── ChimeBackend.Domain/          ← 第二层：领域层（Entities + Enums + Repository接口）
├── ChimeBackend.Infrastructure/   ← 第一层：基础设施层（Repository实现 + DbContext）
└── ChimeBackend.Tests/           ← 测试项目
```

---

## 三、每一层详解

### 3.1 第一层：基础设施层（Infrastructure）

**位置**：`ChimeBackend.Infrastructure/`

**通俗解释**：这一层是"服务员"，专门负责和外部资源打交道。
- 和数据库通信（增删改查）
- 调用外部API（发短信、微信登录）
- 文件读写
- 加密解密

**本项目具体内容**：

```
Infrastructure/
├── Data/
│   ├── ChimeDbContext.cs      ← 业务数据库的"管理员"
│   └── FoodLibraryDbContext.cs ← 食物库的"管理员"
└── Repositories/
    ├── UserRepository.cs       ← 用户数据的"操作员"
    ├── FoodRecordRepository.cs ← 食物记录的"操作员"
    └── ...其他Repository
```

**关键概念：DbContext 是什么？**

可以把它想象成"数据库的遥控器"：

```csharp
// ChimeDbContext.cs 简化版
public class ChimeDbContext : DbContext
{
    public DbSet<User> Users { get; set; }       // Users表
    public DbSet<FoodRecord> FoodRecords { get; set; }  // FoodRecords表

    // 配置实体和表的映射关系
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 告诉EF：User类对应Users表，Id字段对应id列
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");         // 对应哪张表
            entity.HasKey(e => e.Id);        // 主键是什么
            entity.Property(e => e.Id).ValueGeneratedOnAdd(); // 自增
        });
    }
}
```

**Repository 是什么？**

Repository 是"数据操作的封装"。本来你要这样写数据库操作：

```csharp
// 不用Repository的直接写法
var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
dbContext.Users.Add(newUser);
await dbContext.SaveChangesAsync();
```

用了Repository之后，变成这样：

```csharp
// UserRepository.cs
public class UserRepository : IUserRepository
{
    private readonly ChimeDbContext _dbContext;

    public UserRepository(ChimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }
}
```

**为什么要定义接口？**

看 `IUserRepository` 接口：

```csharp
// Domain/Repositories/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**好处**：
1. **应用层不需要知道具体用哪个数据库**
2. **测试时可以用Mock替代真实数据库**
3. **以后换数据库（比如从SQL Server换到MySQL），只需要改Infrastructure层**

---

### 3.2 第二层：领域层（Domain）

**位置**：`ChimeBackend.Domain/`

**通俗解释**：这一层是"规则的制定者"，只定义"是什么"和"能做什么"，不规定"怎么做"。

**本项目具体内容**：

```
Domain/
├── Entities/           ← 实体：业务对象
│   ├── User.cs         ← 用户长什么样
│   ├── FoodRecord.cs   ← 食物记录长什么样
│   ├── Food.cs         ← 食物库里的食物长什么样
│   └── DailySummary.cs ← 每日汇总长什么样
├── Enums/              ← 枚举：固定的选择
│   ├── Gender.cs       ← 性别（男/女）
│   ├── MealType.cs     ← 餐次（早餐/午餐/晚餐/加餐）
│   └── ActivityLevel.cs ← 活动水平
└── Repositories/       ← Repository接口（只定义规矩，不实现）
    ├── IUserRepository.cs
    └── IFoodRecordRepository.cs
```

**Entity（实体）是什么？**

实体是"业务中真正关心的对象"：

```csharp
// Domain/Entities/User.cs
public class User
{
    public int Id { get; set; }              // 唯一标识

    // 认证相关 - 用户怎么登录
    public string? OpenId { get; set; }      // 微信OpenId
    public string? PhoneNumber { get; set; } // 手机号

    // 基础信息 - 用户长什么样
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public Gender? Gender { get; set; }     // 性别是枚举类型
    public decimal? Height { get; set; }     // 身高cm
    public decimal? Weight { get; set; }     // 体重kg
    public int? Age { get; set; }

    // 偏好设置 - 用户想要什么
    public VersionMode VersionMode { get; set; }    // 自律版还是嘴馋版
    public ActivityLevel ActivityLevel { get; set; } // 活动水平
    public int Goal { get; set; }                   // 目标：维持/减脂/增肌
    public decimal? DailyCalorie { get; set; }       // 自定义每日热量

    // 系统字段
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**枚举（Enum）是什么？**

枚举就是"固定的几个选项"：

```csharp
// Domain/Enums/MealType.cs
public enum MealType
{
    Breakfast = 1,  // 早餐
    Lunch = 2,      // 午餐
    Dinner = 3,     // 晚餐
    Snack = 4       // 加餐
}

// 使用时
var mealType = MealType.Breakfast;
if (mealType == MealType.Breakfast)
{
    Console.WriteLine("这是早餐");
}
```

**为什么实体里有很多"?"（可空类型）？**

```csharp
public decimal? Height { get; set; }  // 身高可能没填
public decimal? Weight { get; set; }  // 体重可能没填
public int? Age { get; set; }        // 年龄可能没填
```

表示这些字段"可以没有值"（数据库里是NULL）。

---

### 3.3 第三层：应用层（Application）

**位置**：`ChimeBackend.Application/`

**通俗解释**：这一层是"导演"，指挥具体怎么干活，但自己不亲自动手。

**本项目具体内容**：

```
Application/
├── DTOs/                       ← 数据传输对象（Result DTO）
│   ├── AuthDTOs.cs            ← AuthResult, UserInfoResult
│   ├── FoodDTOs.cs           ← FoodResult, NutritionResult 等
│   ├── FoodRecordDTOs.cs     ← FoodItemInput, DailySummaryResult 等
│   └── UserDTOs.cs           ← UserProfileResult, DailyCalorieResult
└── Services/                  ← 业务逻辑服务
    ├── AuthAppService.cs      ← 认证服务：登录、注册、验证
    ├── UserAppService.cs      ← 用户服务：查资料、改资料、算热量
    ├── FoodRecordAppService.cs ← 食物记录服务：记饮食、查记录、删记录
    └── FoodAppService.cs      ← 食物库服务：搜食物、看分类
```

**AppService 是什么？**

看一个具体的例子 `FoodRecordAppService`：

```csharp
public class FoodRecordAppService
{
    private readonly IFoodRecordRepository _foodRecordRepository;
    private readonly IDailySummaryRepository _dailySummaryRepository;
    private readonly IUserRepository _userRepository;

    // 通过构造函数注入依赖（这些依赖在上面一层定义）
    public FoodRecordAppService(
        IFoodRecordRepository foodRecordRepository,
        IDailySummaryRepository dailySummaryRepository,
        IUserRepository userRepository)
    {
        _foodRecordRepository = foodRecordRepository;
        _dailySummaryRepository = dailySummaryRepository;
        _userRepository = userRepository;
    }

    // 添加食物记录的业务逻辑
    // 注意：参数使用 int 而非 MealType 枚举，避免 Controller 引用 Domain 层
    public async Task<FoodRecordResult> AddRecordAsync(
        int userId,
        DateTime recordDate,
        int mealType,           // ← int 类型，AppService 内部转换
        List<FoodItemInput> foods,
        string? photoUrl,
        string? photoLocalPath,
        string? remark)
    {
        // AppService 内部做类型转换
        var mealTypeEnum = (MealType)mealType;

        // 1. 把用户输入的食物转换成 FoodRecord 实体
        var records = new List<FoodRecord>();
        foreach (var food in foods)
        {
            var record = new FoodRecord
            {
                UserId = userId,
                RecordDate = recordDate,
                MealType = mealTypeEnum,
                FoodName = food.FoodName,
                // 重要：按重量比例计算营养成分
                Calories = food.Energy * food.Weight / 100,  // 热量
                Protein = food.Protein * food.Weight / 100,  // 蛋白质
                Fat = food.Fat * food.Weight / 100,         // 脂肪
                Carbohydrate = food.Carbohydrate * food.Weight / 100,  // 碳水
                // ...
            };
            records.Add(record);
        }

        // 2. 保存到数据库
        await _foodRecordRepository.AddRangeAsync(records);

        // 3. 更新每日汇总
        await UpdateDailySummaryAsync(userId, recordDate);

        // 4. 返回结果
        return new FoodRecordResult(records.First().Id, ...);
    }
}
```

**DTO 是什么？**

DTO = Data Transfer Object（数据传输对象）。简单说就是"只在方法之间传递数据用的类"。

```csharp
// Application/DTOs/FoodRecordDTOs.cs
// 输出DTO：返回给前端的数据（命名规则：Result结尾）
public record FoodRecordResult(
    long Id,
    DateTime RecordDate,
    int MealType,              // ← 用 int 而非枚举
    List<FoodItemResult> Foods,
    string? PhotoUrl
);

// 输入DTO：接收用户提交的数据
public record FoodItemInput(
    long? FoodId,
    string FoodName,
    decimal Weight,
    decimal Energy,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate,
    // ...
);
```

**Api 层 DTO vs Application 层 DTO**

```
Api层 DTOs（用于HTTP请求绑定，带验证特性）
    ↓ 转换
Application层 DTOs（用于服务间传递）
    ↓ 转换
Domain层 实体（最终存储）
```

为什么分开？
- **Api层 Request DTO**：带 `[Required]`、`[StringLength]` 等验证特性，用于接收 HTTP 请求
- **Application层 DTO**：纯数据对象，供 Controller 和 AppService 之间传递
- **好处**：Domain 层类型（枚举等）只在 Application 层内部使用，Controller 完全不依赖 Domain 层
    long? FoodId,
    string FoodName,
    decimal Weight,
    decimal Energy,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate
);

// 输出DTO：返回给前端的数据
public record FoodRecordResult(
    long Id,
    DateTime RecordDate,
    int MealType,
    List<FoodItemResult> Foods,
    string? PhotoUrl
);
```

**为什么要用DTO而不是直接返回实体？**

1. **实体可能很大**，包含很多不需要的数据
2. **实体可能有循环引用**，导致JSON序列化失败
3. **隐藏敏感信息**，比如密码、Token等
4. **解耦**，前端只需要知道返回什么格式

---

### 3.4 第四层：API层（Api）

**位置**：`ChimeBackend.Api/`

**通俗解释**：这一层是"前台接待员"，只负责接收请求和返回响应，不做业务逻辑。

**本项目具体内容**：

```
Api/
├── Controllers/
│   ├── AuthController.cs        ← 认证相关接口
│   ├── UsersController.cs       ← 用户相关接口
│   ├── FoodsController.cs       ← 食物库相关接口
│   └── FoodRecordsController.cs ← 食物记录相关接口
├── DTOs/                       ← 前后端数据传输格式
└── Program.cs                  ← 程序入口配置
```

**Controller 是什么？**

看一个具体的例子 `FoodsController`：

```csharp
// 重点：Controller 只引用 Api层 和 Application层，不引用 Domain层
using ChimeBackend.Api.DTOs;          // Api层 DTO（Request）
using ChimeBackend.Application.DTOs;  // Application层 DTO（Result）
using ChimeBackend.Application.Services;

[ApiController]
[Route("api/foods")]  // 路由：/api/foods
public class FoodsController : ControllerBase
{
    private readonly FoodAppService _foodAppService;

    // 通过构造函数注入应用服务
    public FoodsController(FoodAppService foodAppService)
    {
        _foodAppService = foodAppService;
    }

    // GET /api/foods/categories
    [HttpGet("categories")]
    [AllowAnonymous]  // 不需要登录就能访问
    public async Task<ActionResult<ApiResponse<List<FoodCategoryDto>>>> GetCategories(
        CancellationToken cancellationToken)
    {
        // 1. 调用应用层服务
        var result = await _foodAppService.GetCategoriesAsync(cancellationToken);

        // 2. 转换成前端需要的DTO格式
        var dtos = result.Categories.Select(c => new FoodCategoryDto(
            c.Id,
            c.Title,
            c.CateId,
            c.IsSubcategory,
            c.ParentCategoryId
        )).ToList();

        // 3. 返回统一格式
        return Ok(ApiResponse<List<FoodCategoryDto>>.Success(dtos));
    }

    // GET /api/foods?keyword=米饭&page=1&pageSize=20
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<FoodSearchResultDto>>> SearchFoods(
        [FromQuery] string? keyword,
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _foodAppService.SearchFoodsAsync(
            keyword, categoryId, page, pageSize);

        return Ok(ApiResponse<FoodSearchResultDto>.Success(new FoodSearchResultDto(...)));
    }
}
```

再看一个 `FoodRecordsController` 的例子，展示如何处理枚举类型：

```csharp
// FoodRecordsController.cs
[ApiController]
[Route("api/food-records")]
[Authorize]
public class FoodRecordsController : ControllerBase
{
    private readonly FoodRecordAppService _foodRecordAppService;

    public FoodRecordsController(FoodRecordAppService foodRecordAppService)
    {
        _foodRecordAppService = foodRecordAppService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FoodRecordDto>>> AddRecord(
        [FromBody] AddFoodRecordRequest request,  // Api层 Request DTO
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        // 转换为 Application 层 DTO
        var foods = request.Foods.Select(f => new FoodItemInput(
            f.FoodId,
            f.FoodName,
            f.CategoryId,
            f.CategoryName,
            f.Weight,
            f.Energy,
            f.Protein,
            f.Fat,
            f.Carbohydrate,
            f.Iron,
            f.Sodium,
            f.Price
        )).ToList();

        // 注意：mealType 用 int 传递，不传枚举类型
        var result = await _foodRecordAppService.AddRecordAsync(
            userId,
            request.RecordDate,
            request.MealType,    // ← int 类型，Api层不需要知道 Domain枚举
            foods,
            request.PhotoUrl,
            request.PhotoLocalPath,
            request.Remark,
            cancellationToken);

        return Ok(ApiResponse<FoodRecordDto>.Success(new FoodRecordDto(...)));
    }
}
```

**为什么要用 `[FromQuery]`、`[FromBody]`？**

```csharp
// [FromQuery] 表示参数从URL查询字符串获取
// GET /api/foods?keyword=米饭
public async Task SearchFoods([FromQuery] string? keyword)

// [FromBody] 表示参数从请求体获取
// POST /api/food-records  body: {"recordDate": "2024-01-01", ...}
public async Task<ActionResult> AddRecord([FromBody] AddFoodRecordRequest request)
```

**Program.cs 做了什么？**

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. 添加Controllers服务
builder.Services.AddControllers();

// 2. 配置数据库连接（两个数据库）
builder.Services.AddDbContext<ChimeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChimeDb")));

builder.Services.AddDbContext<FoodLibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodLibraryDb")));

// 3. 配置JWT认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT配置 */ });

// 4. 注册Repository（接口→实现类的映射）
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 5. 注册AppService
builder.Services.AddScoped<FoodRecordAppService>();

var app = builder.Build();

// 6. 配置中间件管道
app.UseSwagger();           // 开启Swagger文档
app.UseAuthentication();   // 开启JWT认证
app.UseAuthorization();     // 开启授权检查
app.MapControllers();       // 路由映射

app.Run();
```

---

## 四、数据流向图

### 4.1 一个典型的"添加食物记录"请求

```
1. 前端发送POST请求
   POST /api/food-records
   Body: {"recordDate": "2024-01-15", "mealType": 1, "foods": [...]}

   ↓ HTTP请求

2. FoodRecordsController接收请求
   - 验证参数格式
   - 从Token中提取userId

   ↓ 调用

3. FoodRecordAppService.AddRecordAsync()
   - 计算营养成分（按重量比例）
   - 保存到数据库
   - 更新每日汇总

   ↓ 调用

4. IFoodRecordRepository
   - 操作ChimeDbContext
   - EF Core生成SQL
   - 执行到SQL Server

   ↓ 数据存储

5. 数据库返回结果，逐层返回给前端
```

### 4.2 一个典型的"搜索食物"请求

```
1. 前端发送GET请求
   GET /api/foods?keyword=米饭&page=1

   ↓ HTTP请求

2. FoodsController接收请求
   - 调用FoodAppService

   ↓ 调用

3. FoodAppService.SearchFoodsAsync()
   - 调用IFoodRepository.SearchAsync()

   ↓ 调用

4. FoodRepository
   - 操作FoodLibraryDbContext（食物库）
   - 执行模糊查询
   - EF Core生成SQL

   ↓ 数据查询

5. 返回食物列表给前端
```

**注意**：食物搜索用的是 `FoodLibraryDbContext`（食物库，只读），
食物记录用的是 `ChimeDbContext`（业务库，可读写）。这是"双数据库"设计。

---

## 五、双数据库设计

### 5.1 为什么要两个数据库？

```
┌─────────────────┐     ┌─────────────────┐
│   ChimeDb       │     │  FoodLibraryDb  │
│  （业务库）           │     │   （食物库）         │
├─────────────────┤     ├─────────────────┤
│ Users表                 │     │ Foods表                 │
│ FoodRecords表      │     │ FoodCategories表 │
│ DailySummaries表│     │                 │
└─────────────────┘     └─────────────────┘
       ↕                      ↕
   增删改查                  只读查询
```

- **ChimeDb（业务库）**：存放用户自己的数据，需要频繁读写
- **FoodLibraryDb（食物库）**：存放30万+条食物营养数据，只需要读取

### 5.2 项目中怎么区分？

```csharp
// 在 Program.cs 中注册两个不同的DbContext
builder.Services.AddDbContext<ChimeDbContext>(...);      // 业务库
builder.Services.AddDbContext<FoodLibraryDbContext>(...); // 食物库

// Repository绑定到不同的DbContext
builder.Services.AddScoped<IUserRepository, UserRepository>();  // 用ChimeDbContext
builder.Services.AddScoped<IFoodRepository, FoodRepository>(); // 用FoodLibraryDbContext
```

---

## 六、依赖规则

### 6.1 箭头方向 = 依赖方向

```
Controller (Api层)
    ↓ 引用
AppService (Application层) ←→ DTOs (Application层)
    ↓ 引用
Repository接口 (Domain层)
    ↑ 实现
Repository实现 (Infrastructure层)
    ↑ 使用
DbContext (Infrastructure层)
```

**核心规则**：
1. Controller **只能引用** Application 层的 DTO（Result）和 AppService
2. Controller **禁止引用** Domain 层的任何类型（实体、枚举）
3. AppService 内部引用 Domain 层完成业务逻辑
4. **禁止反方向调用**（Infrastructure 不能调用 Application）

### 6.2 为什么 Controller 不能引用 Domain 层？

```
错误示范：
Controller → Domain.Enums.MealType  ← 禁止！

正确做法：
Controller → Application层DTO（用int传递枚举值）→ AppService（内部转枚举）
```

**好处**：
- **解耦**：Controller 不需要知道业务层用了什么枚举
- **换枚举不改 Controller**：如果 MealType 改成字符串，Controller 不需要改动
- **测试更简单**：Controller 测试只需要准备 int 值，不需要准备枚举类型

### 6.3 为什么这样规定？

```
假如 Infrastructure 能调用 Application：

    Infrastructure → Application

那么换数据库时（Infrastructure的改动）可能会影响业务逻辑（Application）

这叫"耦合"，是我们要避免的。

正确的设计是 Application 依赖 Infrastructure（接口由Domain定义，具体实现在Infrastructure）：

    Application → Domain ← Infrastructure
                   ↑
            Repository接口    Repository实现
```

---

## 七、认证授权流程

### 7.1 JWT Token 是什么？

JWT = JSON Web Token，简单理解就是"数字身份证"。

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
    ───────────────────────┬─────────────────────────────    ───────────────
            Header.Payload.Signature                        签名（防伪造）
```

组成部分：
1. **Header**：声明类型和算法
2. **Payload**：存放用户信息（userId、昵称等）
3. **Signature**：签名，确保内容没被篡改

### 7.2 本项目Token生成过程

```csharp
// TokenService.GenerateTokens
public (string accessToken, string refreshToken, DateTime expiresAt) GenerateTokens(User user)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),  // 用户ID
        new(ClaimTypes.Name, user.Nickname ?? ""),            // 昵称
        new("versionMode", ((int)user.VersionMode).ToString()) // 版本模式
    };

    var token = new JwtSecurityToken(
        issuer: "ChimeBackend",           // 签发者
        audience: "ChimeBackend",         // 接收者
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),  // 30分钟后过期
        signingCredentials: credentials
    );

    return (new JwtSecurityTokenHandler().WriteToken(token), refreshToken, expiresAt);
}
```

### 7.3 Controller怎么验证Token？

```csharp
[HttpPost("bind-phone")]
[Authorize]  // 需要登录才能访问
public async Task<ActionResult<ApiResponse<AuthResponse>>> BindPhone(
    [FromBody] BindPhoneRequest request)
{
    // 从请求的Token中提取用户ID
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
        return Unauthorized();

    var userId = int.Parse(userIdClaim.Value);

    // 调用服务，传入用户ID
    var result = await _authAppService.BindPhoneAsync(userId, ...);
}
```

---

## 八、单元测试说明

### 8.1 测试的思路

单元测试 = "隔离外部依赖，测试单个方法"

```csharp
// 不测试时：FoodRecordAppService 直接依赖真实的 Repository
FoodRecordAppService
    └── IFoodRecordRepository (真实实现，连接数据库)
    └── IDailySummaryRepository (真实实现，连接数据库)
    └── IUserRepository (真实实现，连接数据库)

// 测试时：用 Mock 替代真实的 Repository
FoodRecordAppService
    └── IFoodRecordRepository (Mock对象，不连接数据库)
    └── IDailySummaryRepository (Mock对象，不连接数据库)
    └── IUserRepository (Mock对象，不连接数据库)
```

### 8.2 Mock是什么？

```csharp
// 创建一个假的对象
var mockRepo = new Mock<IFoodRecordRepository>();

// 设置假对象的假方法
mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new FoodRecord { Id = 1, FoodName = "白米饭" });

// 调用被测试的方法（传入假对象）
var appService = new FoodRecordAppService(mockRepo.Object, ...);
var result = await appService.GetByIdAsync(1);

// 验证结果
result.Should().NotBeNull();
result.FoodName.Should().Be("白米饭");
```

### 8.3 测试覆盖范围

| 测试类 | 覆盖内容 |
|--------|----------|
| FoodRecordAppServiceTests | 食物记录的增删改查、营养计算 |
| AuthAppServiceTests | 登录、注册、Token刷新 |
| UserAppServiceTests | 资料查询、热量计算 |
| FoodAppServiceTests | 食物搜索、分类查询 |

---

## 九、常见问题

### Q1: 为什么不把Repository直接写在AppService里？

因为那样的话，换数据库就要改AppService代码。
分离之后，换数据库只需要改Infrastructure层的Repository实现。

### Q2: DTO和Entity有什么区别？

- **Entity**：对应数据库表，是"真实存在的数据"
- **DTO**：只是方法间传递的数据，可能是Entity的一部分，也可能是多个Entity的组合

### Q3: Api层DTO和Application层DTO有什么区别？

- **Api层DTO（Request）**：放在 `Api/DTOs/` 目录，带 `[Required]`、`[StringLength]` 等验证特性，用于接收 HTTP 请求
- **Application层DTO（Result）**：放在 `Application/DTOs/` 目录，纯数据对象，用于 Controller 与 AppService 之间传递

两者分开的好处：
- 验证逻辑只在 Api 层
- AppService 返回的 DTO 结构可以独立变化，不影响 HTTP 请求格式
- Controller 引用 Application层DTO 时不需要知道 Domain 层类型

### Q4: 什么是"依赖注入"？

简单说就是"不用自己new，让框架帮你创建对象"。

```csharp
// 传统写法
var repo = new UserRepository(new ChimeDbContext());
var service = new UserAppService(repo);

// 依赖注入写法（在Program.cs注册）
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserAppService>();

// Controller构造函数里直接用
public class UsersController
{
    public UsersController(IUserRepository repo)  // 框架自动注入
    {
        _repo = repo;
    }
}
```

### Q4: AddScoped、AddSingleton、AddTransient有什么区别？

| 注册方式 | 创建频率 | 适用场景 |
|----------|----------|----------|
| AddScoped | 每次请求创建一次 | 数据库操作（Repository、DbContext） |
| AddSingleton | 整个程序只创建一次 | 配置类、工具类 |
| AddTransient | 每次注入都创建新实例 | 轻量级服务 |

---

## 十、架构总结

```
┌─────────────────────────────────────────────────────────────────┐
│                         请求进来                                 │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  API层（Controller）                                             │
│  - 接收HTTP请求                                                  │
│  - 提取参数和用户身份                                            │
│  - 返回HTTP响应                                                  │
│  - 不写业务逻辑                                                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  应用层（AppService）                                            │
│  - 写业务逻辑                                                    │
│  - 事务管理                                                      │
│  - 参数校验                                                      │
│  - 调用Repository读写数据                                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  领域层（Domain）                                                │
│  - 定义实体（User、FoodRecord等）                                │
│  - 定义枚举（MealType、Gender等）                               │
│  - 定义Repository接口                                            │
│  - 不写具体实现                                                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  基础设施层（Infrastructure）                                    │
│  - 实现Repository接口                                           │
│  - 操作DbContext读写数据库                                       │
│  - 实现外部服务调用                                              │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  数据库（SQL Server）                                            │
│  - ChimeDb（业务数据）                                          │
│  - FoodLibraryDb（食物库，只读）                                 │
└─────────────────────────────────────────────────────────────────┘
```

**核心思想**：
1. **各层各司其职**：不越界，不乱调用
2. **面向接口编程**：依赖抽象，不依赖具体实现
3. **分离关注点**：改UI不用动业务逻辑，改数据库不用动业务逻辑
4. **易于测试**：每层可以单独测试
5. **DTO解耦**：Controller 与 Domain 完全解耦，通过 int 传递枚举值

---

## 十一、本项目特殊设计

### 11.1 双数据库（CQRS雏形）

```
ChimeDb（业务库）←→ FoodLibraryDb（食物库，只读）
```

业务库存储用户数据和记录，食物库提供30万+食物的营养数据查询。两者物理分离，类似于CQRS的读写分离思路。

### 11.2 枚举传递方式

**错误做法**：
```csharp
// Controller 引用了 Domain 枚举
[HttpPost]
public async Task AddRecord([FromBody] AddFoodRecordRequest request)
{
    await _service.AddRecordAsync(userId, (MealType)request.MealType); // ✗ 禁止！
}
```

**正确做法**：
```csharp
// Controller 不引用 Domain 枚举，用 int 传递
[HttpPost]
public async Task AddRecord([FromBody] AddFoodRecordRequest request)
{
    await _service.AddRecordAsync(userId, request.MealType); // ✓ AppService 内部转换
}
```

枚举转换在 Application 层内部完成，Controller 完全不知道用了什么枚举。

---

*文档版本：v2.0*
*最后更新：2026-04-29*
