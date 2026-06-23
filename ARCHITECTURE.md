# Architecture

本文描述当前实现状态，优先级低于 `docs/REFACTOR_PLAN.md`，但应与代码保持一致。

## 总体结构

```text
WinForms UI
  -> ViewModel
  -> Service
  -> IXxxRepository
  -> Ef / WebApi / Local repository implementation
```

约束：

- UI 只负责控件、布局和交互，不直接访问 DbContext 或 HttpClient。
- ViewModel 只依赖 Service，不依赖 EF、HttpClient、Expression。
- Service 只依赖仓储契约，不依赖具体数据源。
- Repository 实现负责各自数据源细节。

## 启动流程

入口在 `Program.cs`：

1. 初始化 WinForms 和 log4net。
2. `GlobalProjectConfig` 读取 `Resources/Config/config.json`；缺失时从 `config.example.json` 复制。
3. `AppServiceRegistration.BuildServiceProvider` 注册配置、DbContext、仓储、服务、ViewModel、页面控件和导航。
4. `AppServiceRegistration.InitializeDatabaseAsync` 依次执行已注册的 `IDatabaseInitializer`。
5. 解析 `LoginForm`，登录成功后进入 `MainForm`。

初始化失败会 `Debug.Fatal` 并弹出 MessageBox 后退出，避免未处理异常直接崩溃。

## 数据访问

核心抽象在 `Src/Common/DataAccess/`：

- `DataSourceKind`
- `QueryRequest`
- `PagedResult<T>`
- `IRepository<T>`
- `EfRepositoryBase<T>`
- `LocalRepositoryBase<T>`
- `ApiRepositoryBase<T>`
- `DataSourceOptions`
- `DataSourceUnavailableException`
- `ModuleRepositoryServiceCollectionExtensions.AddModuleRepository<...>()`

仓储注册在 `Src/Bootstrap/AppServiceRegistration.cs`：

```csharp
services.AddModuleRepository<IProductRepository, EfProductRepository, ApiProductRepository, LocalProductRepository>("Template");
```

运行时根据 `config.json -> DataSource.Modules` 选择实现。

## 数据源类型

### Ef

EF Core 直连数据库。当前支持：

- SQLite
- MySQL（Pomelo + MySqlConnector）

默认配置为 SQLite。每个 EF 模块使用独立 SQLite 文件：

| 模块 | DbContext | 文件 |
| --- | --- | --- |
| Sys | `SysDbContext` | `Resources/Database/sys.db` |
| Template | `TemplateDbContext` | `Resources/Database/template.db` |

分库原因：EF Core `EnsureCreated` 不支持多个 DbContext 共用同一数据库。若共用一个 SQLite 文件，先执行的 Context 会创建数据库，后执行的 Context 会认为数据库已存在而跳过建表，导致后续访问缺表。模板保留 `EnsureCreated + 自动种子` 的开箱体验，因此选择每模块独立 SQLite 文件。

### Local

Local 仓储从 `Resources/MockData/*.json` 读取种子并保存在内存集合中。CRUD 不回写磁盘，便于演示数据重置。

### WebApi

WebApi 仓储通过 `IWebApiClient` 调用 REST 端点。后端不在本仓库实现；接口契约见 `docs/api-contract.md`。

无后端或传输失败时，WebApi 仓储抛 `DataSourceUnavailableException`。

## 错误模型

仓储失败分两类：

- 业务未命中：返回值表达。例如 `GetByIdAsync -> null`，`QueryAsync -> 空集合`，`Update/DeleteAsync -> false`。
- 数据源不可达：抛 `DataSourceUnavailableException`，包含 moduleKey、endpoint 和 inner exception。

`BaseViewModel.ExecuteAsync` 统一捕获异常并更新 `StatusMessage`。对数据源不可达，UI 呈现“未连接后端”。

## Sys 模块

Sys 模块负责账户、角色、菜单、参数和权限。

关键模型：

- `SysAccountModel`
- `SysRoleModel`
- `SysMenuModel`
- `SysRoleAuthModel`
- `SysParamModel`

字段约定：

- `SysStatus` / `SrStatus` 等状态字段为 `bool?`，`false` 表示有效，`true` 表示冻结或无效。
- 菜单路由 key 使用 `SysMenu.SmUrl`。
- `SysRoleModel` 保留字段为 `SrReserved1`、`SrReserved2`、`SrReserved3`。

默认种子：

- `admin / 123456`：Administrator，授权 `/sys/user`、`/sys/role`、`/template/product`。
- `operator / 123456`：Operator，只授权 `/sys/user`。

## Template / Product 模块

Template 模块当前包含 Product、Category、ImportRecord 等示例业务对象。Product 是“如何新增业务模块”的完整样板。

Product 链路：

- Model：`ProductModel`
- Repository：`IProductRepository` + `EfProductRepository` / `ApiProductRepository` / `LocalProductRepository`
- Service：`ProductService`
- ViewModel：`ProductManagementViewModel`
- UI：`ProductManagementControl`
- 导出：`ProductExcelExporter`

Product 查询通过 `QueryRequest` 下推分页、关键字、排序和过滤。UI 不做 `PageSize=int.MaxValue` 的全量加载后截断。

## 导航与权限

导航抽象在 `Src/Navigation/`：

- `IPageRegistry`
- `PageRegistry`
- `PageRegistryDefaultPages`
- `INavigationService`
- `NavigationService`
- `ICurrentAccountAccessor`

页面注册：

| menuKey | 页面 |
| --- | --- |
| `/sys/user` | `AccountManagementControl` |
| `/sys/role` | `RolePlaceholderControl` |
| `/template/product` | `ProductManagementControl` |

菜单由 `PermissionService.GetAccessibleMenuTreeAsync(accountId)` 按角色过滤。即使直接调用 `INavigationService.NavigateAsync(menuKey)`，也会再次校验当前账户权限。

## MVVM

MVVM 基础设施在 `Src/Common/MVVM/`：

- `ObservableObject`
- `BaseViewModel`
- `RelayCommand`
- `DefaultBindingExtensions`
- `AntdUIBindingExtensions`

LoginForm 已使用 `AntdUIBindingExtensions` 对 Username/Password 做双向绑定。新增 UI 应优先使用这些扩展，避免重复手写 `TextChanged`。

## 配置与凭据

`config.example.json` 入库，使用占位密码。真实 `config.json` 不入库。

默认配置：

- `DataSource.Default = Ef`
- `DataSource.Modules.Sys = Ef`
- `DataSource.Modules.Template = Ef`
- `Ef.DbType = SQLite`
- `Ef.SQLitePath = ./Resources/Database`

`Ef.SQLitePath` 可以是目录，也可以是兼容旧配置的文件路径。传入模块 key 后会解析为 `{module}.db`。

## 安全

- 密码使用 PBKDF2 加盐哈希。
- 默认演示账号只用于模板体验，生产必须修改。
- 数据库真实密码不得提交，`config.example.json` 只放占位符。

## 测试守卫

关键测试：

- `SysMenuSeedUrls_AreConsistentAcrossEfLocalAndRegisteredPages`：EF 菜单种子、Local `sysMenus.json`、页面注册表三方一致。
- `AppStartupIntegrationTests`：使用与 Program 相同的服务注册和初始化顺序，验证默认 Ef 初始化、admin 登录、Product 分页查询。
- `TemplateRepositoryDataSourceTests`：验证 Product Ef/Local 仓储契约一致。
- `ApiRepositoryTests`：验证 WebApi 端点映射和不可达异常。

## 已知限制 / TODO

- 窗口布局尚未做完整自适应/响应式优化；功能可用，视觉待优化。
- SysRole、SysMenu、SysParam 管理列表仍是简单管理入口，后续可参考 Product 页补分页控件。
- WebApi 后端未实现；当前只提供客户端仓储和 REST 契约。
