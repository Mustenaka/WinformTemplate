# WinformTemplate 重构方案（Refactor Plan / 实施规格）

> 本文件是给实现者（codex）落地、给审核员逐条比对 PR 的**唯一规格来源**。
> 实现前请完整阅读；任何与本文件冲突的旧文档（README / ARCHITECTURE）以本文件为准，并在 P5 阶段把它们改齐。

---

## 0. 项目定位与既定决策

**定位**：这是一个**可二次开发的客户端模板**（.NET 8 + WinForms + AntdUI）。使用者会基于它快速派生出很多客户端应用。因此一切设计以「上手即改、零配置可跑、加模块成本低」为最高优先级。

**已拍板的三个关键决策（不要再推翻）**：

1. **数据来源做成「按模块可插拔」**：业务层只依赖仓储抽象 `IXxxRepository`，底下挂三种可互换实现：
   - `Ef`：EF Core 直连（默认 SQLite，可切 MySQL）
   - `WebApi`：走 HTTP 契约（复用现有 `WebApiClient`）
   - `Local`：自写 / 内存 / JSON 数据
   每个**模块**用配置独立选用哪一种，同一个 App 内可混用。
2. **后端暂不实现**：本次只产出客户端的 `WebApi` 仓储实现 + 一份 REST 契约文档。后端项目以后再做，现在不写。
3. **MVVM 用项目自造的那套**（`ObservableObject` / `BaseViewModel` / `RelayCommand`）：完善它，**不要引入 CommunityToolkit.Mvvm 或任何第三方 MVVM 框架**。

---

## 1. 目标架构

```
        UI (AntdUI + UserControl)            ← 上手即改
              │  自造 MVVM 双向绑定
        ViewModel (BaseViewModel / RelayCommand)
              │
        Service (纯业务逻辑，不感知数据来源)
              │  仅依赖抽象
   ┌──────────┴───────────┐
   │   IXxxRepository       │   ← 一个业务模块一份契约
   └──────────┬───────────┘
   ┌──────────┼───────────┬────────────┐
  Ef 实现      WebApi 实现    Local 实现    ← 配置按模块选一个
 (SQLite/      (HttpClient    (内存/JSON/
  MySQL)        + 契约)        自写)
```

需求映射：

| 需求 | 落地方式 |
|---|---|
| 上手即改可用 | 默认 SQLite + 种子数据，首次运行零配置直接跑；新增模块固定 6 步（见 §9） |
| 契合 AntdUI 工具链 | 收口绑定扩展，提供 Table/Form/分页 标准范式 + 一个完整业务样板（Product） |
| DB 连接 + 自写数据 | `Ef`（SQLite/MySQL）与 `Local`（自写/内存/JSON）共用同一契约，配置切换 |
| WebAPI 连接 | `WebApi` 实现复用 `WebApiClient` + REST 契约文档（后端以后做） |
| 按角色开不同页 | 顶层 `IPageRegistry` + `INavigationService`，菜单按角色过滤，页面按 key 解析 |

---

## 2. 关键契约（实现目标 / 审核基准）

放在 `Src/Common/DataAccess/`：

```csharp
public enum DataSourceKind { Ef, WebApi, Local }

// 跨源、可序列化的查询请求（关键：不可用 Expression<Func>，WebAPI 无法传递）
public sealed class QueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
    public Dictionary<string, string>? Filters { get; set; }
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
}

// 通用仓储契约 —— 三种实现都必须满足
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<PagedResult<T>> QueryAsync(QueryRequest req);   // 取代 GetAll + 内存过滤
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(object id);
}
```

**性能红线（直接修掉登录的全表扫描）**：仓储层**之外**不允许出现 `GetAllAsync().Where/FirstOrDefault` 式的内存过滤。每个模块定义**具名下推查询**，三种实现各自原生完成。例如：

```csharp
public interface ISysAccountRepository : IRepository<SysAccountModel>
{
    Task<SysAccountModel?> GetByUsernameAsync(string username); // Ef→WHERE / Api→?username= / Local→字典
}
```

**按模块注册的 DI 扩展（消除样板，按配置选实现）**：

```csharp
// 读配置决定该模块用哪种实现；Ef 实现仅在被选中时才解析，不强依赖 DbContext
services.AddModuleRepository<ISysAccountRepository,
        EfSysAccountRepository, ApiSysAccountRepository, LocalSysAccountRepository>("Sys");
```

实现要点：注册三个具体类为 itself，再为接口注册一个工厂委托，工厂内根据 `DataSourceOptions.Resolve("Sys")` 返回对应实例。

---

## 3. 配置 Schema（按模块切换）

`config.example.json` 入库；真实 `config.json` 加入 `.gitignore`，首次运行若缺失则从 example 复制。

```json
{
  "DataSource": {
    "Default": "Ef",
    "Modules": { "Sys": "Ef", "Template": "Local" }
  },
  "Ef":     { "DbType": "SQLite",
              "SQLitePath": "./Resources/Database/app.db",
              "MySqlConnection": "server=127.0.0.1;port=3306;user=root;database=base;password=__SET_ME__;" },
  "WebApi": { "BaseUrl": "https://localhost:5001", "TimeoutSeconds": 30 },
  "Local":  { "SeedPath": "./Resources/MockData" }
}
```

模块未列出则用 `Default`。**验收：把 `Template` 改成 `WebApi` 或 `Local`，不改一行业务代码即可切换。**

---

## 4. 权限页面路由（需求 5）

```csharp
public interface IPageRegistry
{
    void Register(string menuKey, Func<IServiceProvider, UserControl> factory);
    bool TryResolve(string menuKey, IServiceProvider sp, out UserControl page);
}

public interface INavigationService
{
    Task<bool> NavigateAsync(string menuKey); // 内部再做一次权限校验
}
```

- 启动时把页面工厂按 `menuKey`（对应 `SysMenu.SmUrl`，或给菜单新增 `SmKey` 字段）注册进 `IPageRegistry`。
- `MainForm` 删除现有空 `switch`（`Tab_Main_SelectedIndexChanged` 里 `case 0/1: break;`），改为：
  1. 菜单树由 `PermissionService.GetAccessibleMenuTreeAsync(accountId)` **按当前角色过滤**后渲染；
  2. 选中菜单时 `INavigationService.NavigateAsync(menuKey)` 从注册表解析 UserControl；
  3. 解析不到 → 显示「功能未实现」占位页；无权限 → 显示「无权限」占位页。
- **纵深防御**：导航时再查一次 `HasPermissionAsync`，防止越权直达。

**验收场景**：admin 看到全部菜单且页面均可打开；另建一个只授权了部分菜单的低权角色，登录后**只能看到并打开被授权页面**，其余不可见、直达被拒。

---

## 5. 目录结构（按模块内聚，方便「加一个模块」）

```
WinformTemplate/Src/
├── Common/
│   ├── MVVM/            # 完善 ObservableObject/BaseViewModel/RelayCommand/AsyncRelayCommand + 绑定扩展
│   ├── DataAccess/      # DataSourceKind / QueryRequest / PagedResult / IRepository /
│   │                    #   EfRepositoryBase / AddModuleRepository / DataSourceOptions /
│   │                    #   IDatabaseInitializer / WebApiClient(迁入)
│   ├── Patterns/  Config/
├── Navigation/          # IPageRegistry / INavigationService / 占位页
├── Business/<模块>/
│   ├── Model/
│   ├── Repositories/
│   │   ├── IXxxRepository.cs            # 契约
│   │   ├── EfCore/EfXxxRepository.cs    # 直连
│   │   ├── WebApi/ApiXxxRepository.cs   # 走契约
│   │   └── Local/LocalXxxRepository.cs  # 自写
│   ├── Service/   └── ViewModel/
└── UI/Business/<模块>/  # UserControl + Designer
WinformTemplate.Tests/    # 独立测试工程（从主工程移出）
docs/                     # 本规格 + REST 契约 + 二开指南
```

---

## 6. 现有代码处置表

| 处置 | 对象 |
|---|---|
| **删除** | `Src/Business/Sys/Context/` 下 6 个 per-model Context（`SysAccountContext` 等）+ `Context/Mgr/AllFactoryContext.cs` + `Context/ContextQuestion.md`（死代码、硬编码 MySQL、与单 Context 路线冲突）；无人读取的 `Resources/Setting/Setting.yaml`；`MySql.Data` 依赖（保留 `Pomelo.EntityFrameworkCore.MySql` + `MySqlConnector`） |
| **改造保留** | `WebApiClient` → 迁入 `Common/DataAccess/`，注册 DI，作为 `Api*Repository` 的传输层；`SysDbContextService` 的种子逻辑 → 抽成 `IDatabaseInitializer`（仅当存在 Ef 模块时执行），不再作为捕获 `IServiceProvider` 的 Singleton 服务定位器；`Context/Full/SysDbContext.cs` 单 Context 保留 |
| **修复** | `Program.cs`：`MainForm` 只注册一次、`LoginForm` 走 DI 解析（勿手动 `new`）；`SysAccountModel.SysRoleId` 等 `long`/`long?` 统一（消除「恒为 false」的权限判断警告）；37 条编译 warning 清零；源码 mojibake（如 `Program.cs` 注释乱码） |
| **安全** | 密码哈希改 PBKDF2 或 BCrypt 加盐（保留对旧 MD5 的校验兼容 + 一个迁移开关）；数据库凭据移出入库配置（example 用占位符）；`admin/123456` 作为模板默认保留，但 README 显著标注「生产务必修改」 |
| **数据库初始化** | 默认 `EnsureCreatedAsync` + 种子（保「上手即用」）；MySQL / schema 演进场景把 EF Migrations 作为**可选**路径文档化；README/ARCHITECTURE 改为与实现一致（默认 SQLite、字段类型、命名） |

---

## 7. 分阶段 TODO 与验收标准

> 实现策略：**逐阶段提交，每阶段结束停下来等审核**。未通过审核不要进入下一阶段。每阶段保持 `dotnet build` 绿色。

### P0 清理定型
- 删死代码、修 DI、拆独立测试工程、清冗余 MySQL 驱动、升级 log4net、凭据出库。
- ✅ 验收：解决方案 0 error、warning 显著下降；`dotnet test` 可在独立工程运行；无残留 per-model Context / WebApi 死引用。

### P1 数据访问抽象层（先让 Sys 模块跑在新抽象上）
- 落地 `IRepository<T>` / `QueryRequest` / `PagedResult` / `EfRepositoryBase` / `AddModuleRepository` / `DataSourceOptions` / `IDatabaseInitializer`；Sys 全模块改用具名下推查询。
- ✅ 验收：登录不再全表扫描（开 EF SQL 日志确认带 `WHERE`）；Sys 模块默认 SQLite 跑通 登录 → 加载菜单。

### P2 WebApi + Local 双实现 + 按模块切换
- 为 Sys / Template 各补 `Api*` 与 `Local*` 实现；Local 从 `Resources/MockData` 读 JSON；产出 `docs/api-contract.md`（REST 约定 + `ApiResponse<T>` 包络 + 各端点 URL/方法/请求/响应样例）。
- ✅ 验收：改配置把某模块切到 `Local`，业务零改动跑通；切到 `WebApi` 且无后端时给出明确「未连接后端」降级提示而非崩溃。

### P3 权限页面路由
- `IPageRegistry` / `INavigationService`，替换 `MainForm` 空 switch；菜单按角色过滤。
- ✅ 验收：见 §4 多角色场景。

### P4 业务样板 + AntdUI 绑定收口
- 实现 `ProductManagementControl` 全链路（AntdUI `Table` 列表 + 分页 + 增删改 + 导出 Excel/NPOI），作为「如何加一个业务模块」的范例；`LoginForm` 等改用绑定扩展替换手工 `TextChanged` 接线。
- ✅ 验收：Product 页增删改查 + 导出可用；§9 的 6 步被该样板完整覆盖。

### P5 质量与文档
- warning 清零；README / ARCHITECTURE 与实现对齐；补 `docs/二开指南.md`。
- ✅ 验收：文档描述与实际行为一致；照指南能在 30 分钟内加出一个新模块。

---

## 8. 审核红线（审核员逐条检查）

1. **抽象不泄漏**：Service / ViewModel 内不得出现 `DbContext`、`HttpClient`、EF 类型或 `Expression<Func>`。
2. **下推查询**：仓储层之外无 `GetAll().Where/FirstOrDefault` 式内存过滤。
3. **切换无侵入**：换数据源只动配置；三种实现满足同一契约且行为一致（同一套用例应能跑通三种实现）。
4. **权限闭环**：菜单按角色过滤 + 导航二次校验都在。
5. **上手即用**：`git clone` → F5，默认 SQLite 直接跑通登录，无需手工建库。
6. **安全**：无明文密码入库；哈希加盐。
7. **MVVM 约束**：未引入第三方 MVVM 框架；ViewModel 全部基于自造基类。

---

## 9. 「新增一个业务模块」的 6 步（模板核心体验，P4 样板需覆盖）

1. `Business/<模块>/Model/`：写实体。
2. `Repositories/IXxxRepository.cs`：定义契约（含具名下推查询）。
3. `Repositories/{EfCore,WebApi,Local}/`：写三种实现（Local 可先返回 mock）。
4. `Service/`：写纯业务逻辑（只依赖契约）。
5. `ViewModel/`：继承 `BaseViewModel`，用 `RelayCommand` 暴露命令。
6. `UI/Business/<模块>/`：做 UserControl，绑定 VM；在 `Program.cs` 用 `AddModuleRepository<...>("<模块>")` 注册，并把页面工厂注册进 `IPageRegistry`。

---

**版本**：v1.0（2026-06-23）· 实现：codex · 审核：Claude
