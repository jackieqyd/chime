# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

舌尖账本是一款健康饮食管理应用，提供拍照记录、食物搜索、热量分析功能，支持自律版和嘴馋版两种模式。

- **后端**：ASP.NET Core 9.0 WebAPI（DDD架构）
- **小程序**：uniapp（微信小程序，frontend待完善）
- **双数据库**：业务库(ChimeDb) + 食物库(QwTasteNote)

## 常用命令

```bash
# 后端构建
cd src/Backend
dotnet build ChimeBackend.sln

# 运行API（开发环境监听5015端口，HTTPS）
dotnet run --project ChimeBackend.Api/ChimeBackend.Api.csproj

# 运行所有测试
dotnet test ChimeBackend.Tests/ChimeBackend.Tests.csproj

# 运行单个测试（按方法名筛选）
dotnet test ChimeBackend.Tests/ChimeBackend.Tests.csproj --filter "FullyQualifiedName~TestMethodName"

# 迁移数据库
dotnet ef migrations add InitialCreate --project ChimeBackend.Infrastructure --startup-project ChimeBackend.Api
dotnet ef database update --project ChimeBackend.Infrastructure --startup-project ChimeBackend.Api
```

**配置文件**：`src/Backend/ChimeBackend.Api/appsettings.json`

## 架构说明

### 项目结构（四层DDD + UT）

```
src/Backend/ChimeBackend.sln
├── ChimeBackend.Domain/              # 领域层：实体、枚举、Repository接口
├── ChimeBackend.Application/         # 应用层：AppService + DTOs
│   ├── DTOs/                       # 数据传输对象（供Api层引用）
│   └── Services/                    # 业务逻辑服务
├── ChimeBackend.Infrastructure/      # 基础设施层：Repository实现、EF Core
├── ChimeBackend.Api/                # API层：Controller（只调用AppService）
└── ChimeBackend.Tests/              # 单元测试：xUnit + Moq + FluentAssertions
```

### 依赖规则

```
Controller (Api层) → AppService (Application层) → Repository接口 (Domain层) → Repository实现 (Infrastructure层) → DbContext
                      ↑
                   DTOs（独立目录，Api层可直接引用）
```

**重要规则**：
- Controller 只能调用 Application 层服务，禁止直接使用 DbContext 或 Repository
- Controller 与 AppService 之间通过 DTO 传递数据
- **禁止 Controller 引用 Domain 层类型**（如枚举、实体），枚举值用 int 传递，由 AppService 内部转换
- Api 层 DTO（Request/Response）用于 HTTP 绑定，Application 层 DTO 用于服务间传递

### 双数据库配置

Program.cs 中配置了两个 DbContext：
- `ChimeDbContext` → 连接 ChimeDb（业务数据：用户、食物记录、每日汇总）
- `FoodLibraryDbContext` → 连接 QwTasteNote（只读食物库，CQRS读侧）

连接字符串在 `appsettings.json` 的 ConnectionStrings 节。

### Repository接口（Domain层）

| 接口 | 说明 |
|------|------|
| `IUserRepository` | 用户数据访问 |
| `IFoodRecordRepository` | 食物记录数据访问 |
| `IDailySummaryRepository` | 每日汇总数据访问 |
| `IFoodRepository` | 食物库数据访问 |
| `IFoodCategoryRepository` | 食物分类数据访问 |

### AppService（Application层）

| 服务 | 说明 |
|------|------|
| `AuthAppService` | 认证服务：登录、验证码、Token刷新 |
| `UserAppService` | 用户服务：资料管理、热量计算 |
| `FoodRecordAppService` | 食物记录服务：CRUD、每日汇总 |
| `FoodAppService` | 食物库服务：搜索、分类、详情 |

### 关键实体

| 实体 | 数据库表 | 说明 |
|------|----------|------|
| User | Users | 用户，存储OpenId/UnionId/手机号 |
| FoodRecord | FoodRecords | 食物记录，含营养成分快照 |
| DailySummary | DailySummaries | 每日汇总预聚合数据 |
| Food | Foods（食物库） | 来自QwTasteNote的30+营养素食物，热量字段为 `Energy` |

### 枚举定义

- `MealType`: 1-早餐, 2-午餐, 3-晚餐, 4-加餐
- `VersionMode`: 0-自律版, 1-嘴馋版
- `ActivityLevel`: 活动水平（久坐/轻度/中度/高度/极端）
- `Gender`: Male, Female

### 热计算公式

用户推荐热量通过BMR×活动系数计算：
- BMR（男）= 66.47 + (13.75×体重kg) + (5.003×身高cm) - (6.755×年龄)
- BMR（女）= 655.1 + (9.563×体重kg) + (1.85×身高cm) - (4.676×年龄)
- TDEE = BMR × 活动系数
- 推荐值根据Goal调整：维持=TDEE，减脂=TDEE×0.8，增肌=TDEE×1.1

## API模块

### 认证模块 `/api/auth`

| 接口 | 方法 | 说明 |
|------|------|------|
| `/api/auth/code` | POST | 发送手机验证码 |
| `/api/auth/phone-login` | POST | 手机号+验证码登录 |
| `/api/auth/miniprogram-login` | POST | 微信小程序登录（开发环境用code模拟） |
| `/api/auth/apple-login` | POST | Apple登录 |
| `/api/auth/bind-phone` | POST | 绑定手机号（需认证） |
| `/api/auth/refresh` | POST | 刷新Token |

### 食物模块 `/api/foods`

| 接口 | 方法 | 说明 |
|------|------|------|
| `/api/foods/categories` | GET | 获取食物分类列表 |
| `/api/foods?keyword=xxx` | GET | 搜索食物（支持分页） |
| `/api/foods/{id}` | GET | 获取食物营养成分详情 |

### 食物记录模块 `/api/food-records`

| 接口 | 方法 | 说明 |
|------|------|------|
| `POST /api/food-records` | POST | 添加食物记录（需认证） |
| `GET /api/food-records` | GET | 查询记录列表（需认证） |
| `PUT /api/food-records/{id}` | PUT | 更新记录（需认证） |
| `DELETE /api/food-records/{id}` | DELETE | 删除记录（需认证） |
| `GET /api/food-records/summary` | GET | 获取每日汇总（需认证） |

### 用户模块 `/api/users`

| 接口 | 方法 | 说明 |
|------|------|------|
| `GET /api/users` | GET | 获取用户信息（需认证） |
| `PUT /api/users` | PUT | 更新用户信息（需认证） |
| `GET /api/users/daily-calorie` | GET | 获取推荐热量（需认证） |

## 小程序前端

前端项目位于 `src/WXApp/ChimeWXApp`，使用 uniapp 开发。

**注意**：小程序前端调用后端API时，需要同步更新接口路径以匹配后端Restful规范。

## 开发注意事项

- **架构遵循**：Controller → AppService → Repository → DbContext，禁止跨层调用
- **DTO传递**：Controller与AppService之间传递DTO，不暴露Domain实体
- **FoodRecord** 存储营养成分快照，不直接关联食物库（避免历史数据漂移）
- **食物库列名**：EF Core在 `FoodLibraryDbContext.cs` 中配置snake_case映射（如 `food_name`, `cate_id`）
- API默认运行在 https://localhost:5015
- 开发环境验证码打印到控制台，生产环境需接入短信网关

### 数据库与EF Core类型一致性（重要）

修改数据库字段类型后，必须同步检查 `ChimeDbContext.cs` 中的 EF Core 配置。枚举类型字段（如 Gender、VersionMode、ActivityLevel、Goal、Status、MealType）如果配置了 `HasColumnType("tinyint")`，但数据库实际是 `int`，会导致 `InvalidCastException: Unable to cast object of type 'System.Int32' to type 'System.Byte'`。

**排查要点**：
- 搜索所有 `HasColumnType("tinyint")` 配置，与数据库实际类型逐一比对
- 检查实体类的导航属性，关联表查询时也会触发类型转换
- 建议：枚举字段统一使用 `HasConversion<int>()` 让EF Core自动处理类型映射
- 修改配置后必须重启后端进程使新配置生效
