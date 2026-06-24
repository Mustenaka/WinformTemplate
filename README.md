# WinformTemplate

WinformTemplate 是一个 .NET 8 + WinForms + AntdUI 的客户端模板。当前目标是开箱即用、便于二次开发，并提供一个完整 Product 模块作为新增业务模块范例。

## 当前能力

- WinForms 桌面客户端，UI 使用 AntdUI。
- 自造 MVVM 基础设施：`ObservableObject`、`BaseViewModel`、`RelayCommand`，未引入第三方 MVVM 框架。
- 按模块可插拔数据源：每个业务模块通过 `IXxxRepository` 抽象访问数据，可在 `Ef`、`WebApi`、`Local` 三种实现之间切换。
- 默认 SQLite 首次运行自动建库和种子，直接登录进入菜单。
- 权限导航：菜单按角色过滤，页面按 `SysMenu.SmUrl` 作为 key 解析，导航时二次鉴权。
- Product 管理范例：列表分页、搜索、排序、增删改和 Excel 导出。

## 快速开始

环境要求：

- Windows
- .NET 8 SDK

命令：

```powershell
dotnet restore
dotnet build WinformTemplate.sln -warnaserror
dotnet test WinformTemplate.sln
dotnet run --project WinformTemplate
```

首次运行时，程序会从 `WinformTemplate/Resources/Config/config.example.json` 复制生成本地 `config.json`。真实 `config.json` 不入库，数据库密码等凭据应只保存在本地文件里。

## 演示账号

默认种子账号：

| 账号 | 密码 | 权限 |
| --- | --- | --- |
| `admin` | `123456` | 可访问 `/sys/user`、`/sys/role`、`/template/product` |
| `operator` | `123456` | 只可访问 `/sys/user` |

生产环境必须修改默认密码。密码使用 PBKDF2 加盐哈希；配置项 `Security.UpgradeLegacyPasswordHashOnLogin` 用于兼容并迁移旧哈希。

## 数据源切换

数据源配置在 `Resources/Config/config.json`：

```json
{
  "DataSource": {
    "Default": "Ef",
    "Modules": {
      "Sys": "Ef",
      "Template": "Ef"
    }
  },
  "Ef": {
    "DbType": "SQLite",
    "SQLitePath": "./Resources/Database",
    "MySqlConnection": "server=127.0.0.1;port=3306;user=root;database=base;password=__SET_ME__;"
  },
  "WebApi": {
    "BaseUrl": "https://localhost:5001",
    "TimeoutSeconds": 30
  },
  "Local": {
    "SeedPath": "./Resources/MockData"
  }
}
```

`DataSource.Modules` 可按模块切换：

- `Ef`：直连 EF Core，支持 SQLite 和 MySQL。默认 SQLite。
- `Local`：从 `Resources/MockData/*.json` 读取本地内存数据，CRUD 只在进程内生效。
- `WebApi`：按 `docs/api-contract.md` 调用 REST 端点。后端本仓库暂未实现，当前只有客户端仓储和契约。

示例：只把 Template 切到 Local：

```json
"Modules": {
  "Sys": "Ef",
  "Template": "Local"
}
```

## SQLite 分库设计

默认 SQLite 下，每个 EF 模块使用独立数据库文件：

- Sys 模块：`Resources/Database/sys.db`
- Template 模块：`Resources/Database/template.db`

原因：EF Core `EnsureCreated` 不支持多个 `DbContext` 共用同一个数据库文件。若 `SysDbContext` 先用 `EnsureCreated` 建了库，`TemplateDbContext` 再执行时会认为数据库已存在，不会创建自己的表，随后访问 `product_category` 等表会失败。模板选择“每个 EF 模块一个 SQLite 文件”，保持首次运行自动建库和种子简单可靠。

如果二开项目必须使用单一数据库文件，应改用 EF Migrations 统一建表，而不是继续使用多个 DbContext 的 `EnsureCreated`。

## 错误模型

仓储区分两类失败：

- 正常业务未命中：返回 `null`、`false` 或空集合。例如找不到记录、删除影响 0 行。
- 数据源不可达：抛 `DataSourceUnavailableException`。例如 WebApi 后端未启动、连接超时、传输失败。

`BaseViewModel.ExecuteAsync` 会统一捕获 `DataSourceUnavailableException` 并呈现“未连接后端”。因此读操作返回空集合只表示“确实没数据”，不会被用来伪装后端不可达。

## 菜单与页面 key

菜单 key 使用 `SysMenu.SmUrl`。当前已注册页面：

| key | 页面 |
| --- | --- |
| `/sys/user` | `AccountManagementControl` |
| `/sys/role` | 角色占位页 |
| `/template/product` | `ProductManagementControl` |

EF 种子、Local MockData、页面注册表必须保持一致。守卫测试 `SysMenuSeedUrls_AreConsistentAcrossEfLocalAndRegisteredPages` 会检查这一点。

## 主要目录

```text
WinformTemplate/
  Program.cs
  MainForm.cs
  Resources/
    Config/config.example.json
    MockData/*.json
  Src/
    Bootstrap/AppServiceRegistration.cs
    Common/DataAccess/
    Common/MVVM/
    Business/Sys/
    Business/Template/
    FIO/Excel/
    Navigation/
  UI/
    Business/Sys/
    Business/Template/Product/
WinformTemplate.Tests/
  Navigation/
  Startup/
  Business/
docs/
  REFACTOR_PLAN.md
  api-contract.md
  二开指南.md
```

## 二开入口

新增业务模块请从 Product 模块对照：

- Model：`Src/Business/Template/Model/ProductModel.cs`
- 仓储契约：`Src/Business/Template/Repositories/IProductRepository.cs`
- 三种实现：`Repositories/EfCore/`、`Repositories/WebApi/`、`Repositories/Local/`
- Service：`Src/Business/Template/Service/ProductService.cs`
- ViewModel：`Src/Business/Template/ViewModel/ProductManagementViewModel.cs`
- UI：`UI/Business/Template/Product/ProductManagementControl.cs`
- 注册：`Src/Bootstrap/AppServiceRegistration.cs`
- 页面：`Src/Navigation/PageRegistryDefaultPages.cs`

完整步骤见 [docs/二开指南.md](docs/二开指南.md)。

## 测试

关键测试：

- `NavigationPermissionTests`：权限菜单、页面 key、EF/Local 菜单种子一致性。
- `AppStartupIntegrationTests`：用真实 DI 和初始化顺序验证 Sys + Template EF 首次启动、登录、Product 查询。
- `TemplateRepositoryDataSourceTests`：Product 仓储 Ef/Local 契约一致。
- `ProductManagementViewModelTests`：Product 分页、CRUD 和导出逻辑。

## 已知限制 / TODO

- 窗口布局尚未做完整自适应/响应式优化；功能可用，视觉和小屏体验待优化。
- SysRole、SysMenu、SysParam 的管理列表仍是简单全量管理入口，后续可参考 Product 页补分页 UI。
- WebApi 后端未在本仓库实现；当前仅提供客户端仓储和 `docs/api-contract.md`。

## 许可证

MIT。见 [LICENSE](LICENSE)。
