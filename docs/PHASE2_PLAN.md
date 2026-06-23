# Phase II 计划 — 体验与案例完善（Plan / 实施规格）

> 承接已完成的 P0–P5 可插拔数据源重构（见 docs/REFACTOR_PLAN.md）。本阶段把模板从“架构干净”
> 推进到“好用、好二开、能 vibe coding”。仍是：审核员写规格、codex 落地、审核员逐条比对。
> 与旧文档冲突时以本文件为准。

---

## 0. 背景与既定决策（已拍板，勿推翻）

- **后端独立成项目**：在 `D:\Work\Code\CSharp\WinformTemplateServer\`（当前空目录）新建 ASP.NET Core
  后端，**与客户端仓库分开**（独立 git 仓库）。目标框架 **.NET 8**。Minimal API + EF Core + SQLite。
  它实现 WebAPI 示例所需的 REST 端点，使 WebApi 数据源能端到端跑 CRUD。
- **三种数据源各一个独立 CRUD 案例**：用**同一个简单实体**（命名 `DemoNote`，中文“便签”），做**三个独立示例页**
  —— EF 示例页 / WebAPI 示例页 / Local 示例页，每页**固定绑定一种**数据访问实现，演示同一套 CRUD 在三种
  底层下的写法差异。尽量用上 AntdUI（表格、增删改、搜索/分页）。
- **依赖升级走稳妥路线**：保持 **.NET 8 LTS**；各 NuGet 包升到各自**大版本内最新稳定版**；修已知漏洞；
  不升 .NET 9、不做主版本大跳。
- **MVVM 仍用自造那套**；红线沿用 P 阶段（见 §8）。

---

## 1. 范围与阶段

| 阶段 | 内容 |
|---|---|
| **II-1 依赖升级** | 客户端 NuGet 包在 .NET 8 内升最新、修漏洞，build/test 全绿（基线） |
| **II-2 后端项目** | 新建 `WinformTemplateServer`（ASP.NET Core Minimal API + EF + SQLite），实现 DemoNote 的 REST |
| **II-3 三种数据源 CRUD 案例** | 一个 DemoNote 实体 + 三个示例页（EF/WebAPI/Local），菜单接入 + 权限 + 种子；WebAPI 页连 II-2 后端 |
| **II-4 UI 优化** | 全部窗体自适应/响应式布局；账户管理补分页 UI；AntdUI 主题/间距统一 |
| **II-5 文档 + Skill** | 重组架构介绍、二开文档；产出一个 Claude Code skill 支撑 vibe coding |

每阶段独立提交、做完停下等审核；保持 `dotnet build -warnaserror` 0/0、`dotnet test` 全绿、
既有守卫测试与启动集成测试不被破坏。

---

## 2. II-1 依赖升级

- 客户端 `WinformTemplate.csproj` / `WinformTemplate.Tests.csproj` 的包升到 .NET 8 内最新稳定：
  EF Core 8.x（含 Sqlite/Tools/Pomelo）、AntdUI、NPOI、CsvHelper、log4net、MySqlConnector、
  Newtonsoft.Json、NUnit 系列、Microsoft.NET.Test.Sdk 等。
- `dotnet list package --vulnerable --include-transitive` 无漏洞。
- **不引入新功能**，纯升级 + 必要的 API 适配。
- ✅ 验收：升级为单独提交；build -warnaserror 0/0；全部既有测试通过；运行客户端能正常启动到登录。

---

## 3. II-2 后端项目（WinformTemplateServer）

位置：`D:\Work\Code\CSharp\WinformTemplateServer\`，独立 `git init`，独立 sln。

- ASP.NET Core（.NET 8）**Minimal API** + EF Core + **SQLite**（服务端自有库，不与客户端共享文件）。
- 实现 **DemoNote** 的 REST，端点/响应**严格遵循** `docs/api-contract.md` 的约定：
  统一 `ApiResponse<T>` 包络、分页 `?page=&pageSize=&keyword=&sortBy=&desc=`、`isTransportError` 语义、
  CRUD 端点 `GET /api/Demo/notes`、`GET /api/Demo/notes/{id}`、`POST/PUT/DELETE`。
  （本阶段把 api-contract.md 扩展出 DemoNote 段落，作为客户端与后端的唯一契约来源。）
- 启动即可用：首次运行自动建库 + 少量种子；提供 `dotnet run` 即能在 `WebApi.BaseUrl`（默认
  `https://localhost:5001` 或 http 等价）上响应。
- CORS / Swagger（开发期）可选但推荐，便于自测。
- 基本测试：端点级集成测试（WebApplicationFactory）覆盖 DemoNote 的 CRUD + 分页。
- ✅ 验收：`dotnet run` 起后端，`curl` 能跑通 DemoNote 增删改查与分页；响应结构与 api-contract.md 一致；
  后端自身 build/test 绿。

---

## 4. II-3 三种数据源 CRUD 案例（客户端）

- 新增 `Demo` 模块：实体 `DemoNote`（字段示例：Id、Title、Content、Pinned(bool)、CreateAt、UpdateAt）。
- 仓储契约 `IDemoNoteRepository : IRepository<DemoNote>`（含具名下推查询，如按 Title 关键字）。
  三实现：`EfDemoNoteRepository`、`ApiDemoNoteRepository`、`LocalDemoNoteRepository`。
- **三个独立示例页**（UI/Business/Demo/ 下），每页固定一种实现、各自完整 CRUD：
  - `EfDemoNoteControl` → 直连 SQLite（Demo 模块的 Ef 库，独立文件 demo.db）。
  - `ApiDemoNoteControl` → 走 `ApiDemoNoteRepository` 连 II-2 后端。
  - `LocalDemoNoteControl` → 走 `LocalDemoNoteRepository`，读 `Resources/MockData/demoNotes.json`。
  每页：AntdUI 表格 + 增/改/删 + 关键字搜索 + 分页；按钮/输入尽量用绑定扩展。
- 为了“三页各演示一种、互不影响”，这三页的数据源**在注册时固定**（不依赖 DataSource.Modules 切换），
  并在页面上明确标注“当前数据源：EF / WebAPI / Local”。这是有意为之的教学型展示。
- 菜单接入：新增 Demo 菜单分组 + 三个子菜单（key 如 `/demo/note-ef`、`/demo/note-api`、`/demo/note-local`），
  EF 种子（SysDatabaseInitializer）与 Local 种子（MockData）+ roleAuths **同步新增**、保持
  `SysMenuSeedUrls` 守卫测试绿；在 `PageRegistryDefaultPages` 注册这三页；admin 授权三个、operator 自定。
- 降级：WebAPI 页在后端**未启动**时显示“未连接后端”（沿用 DataSourceUnavailableException → StatusMessage），
  不崩溃；EF/Local 两页与后端无关，后端关着也能正常 CRUD。
- ✅ 验收：后端开着时三页都能完整 CRUD；后端关着时 EF/Local 两页照常、WebAPI 页给出明确未连接提示；
  菜单/权限/种子一致（守卫测试绿）；ViewModel 逻辑（CRUD/分页/搜索）有单元测试，Ef/Local 仓储有用例。

---

## 5. II-4 UI 优化

- **响应式/自适应布局**：LoginForm、MainForm、Product 页、账户管理页、三个 Demo 页在缩放/最大化时
  控件随窗体自适应（Anchor/Dock/TableLayoutPanel 等），设置合理 MinimumSize；不再出现控件错位/留白。
- **账户管理补分页 UI**：把 P5 记的“账户列表只取首页 20 条”补上分页控件（参照 Product 页），
  调 `QueryAccountsAsync(keyword, page, pageSize)`。
- AntdUI 主题/圆角/间距/字体统一，整体观感一致；登录、主界面、各业务页风格协调。
- ✅ 验收：各窗体拖拽缩放/最大化布局正常、无明显错位；账户管理可翻页；视觉统一。
  （UI 视觉这层靠人工 /run 冒烟确认，自动化只覆盖 ViewModel 逻辑。）

---

## 6. II-5 文档 + Skill

- **重组架构介绍**：刷新 ARCHITECTURE.md（或新增总览），覆盖：客户端分层 + 可插拔数据源 + 独立后端 +
  三种数据源案例 + 权限路由 + 每模块独立 SQLite 的原因；给一张清晰的整体架构图与数据流。
- **二次开发文档**（docs/二开指南.md 扩写或新文件）：以 DemoNote 三页为范例，分别讲清
  “怎么加一个 EF 模块 / 怎么接一个 WebAPI 模块（含后端端点约定）/ 怎么用 Local 自写数据”；
  含踩坑 checklist（菜单种子守卫、独立 EF 库、契约一致、降级语义）。
- **Claude Code Skill**（`.claude/skills/winform-template-dev/SKILL.md`）：把本模板的约定固化成 skill，
  让 vibe coding 的 agent 能据此正确二开 —— 内容含：项目定位、目录约定、加模块 6 步、三种数据源选择与写法、
  红线（抽象不泄漏/下推查询/配置切换/权限闭环/独立 EF 库/PBKDF2/自造 MVVM）、必跑的测试（守卫/启动集成）。
  skill 描述要让相关二开任务能稳定触发。
- ✅ 验收：文档与实现一致（抽查后端契约、菜单 key、分库说明、三页数据源）；skill 结构合法、描述清晰，
  跟着它能正确加出一个新模块。

---

## 7. 审核红线（逐条检查）

1. 抽象不泄漏：Service/ViewModel/UI 无 `DbContext`/`HttpClient`/EF 类型/`Expression<Func>`。
2. 查询下推：仓储外无 `GetAll().Where/FirstOrDefault`。
3. **客户端与后端共享同一份 REST 契约**（api-contract.md 为准）；ApiDemoNoteRepository 与后端端点一致。
4. 后端不是唯一路径：EF/Local 示例在后端关闭时仍可用；WebAPI 示例在后端关闭时优雅降级、不崩。
5. 菜单/页面 key 在 EF 种子、Local 种子、注册表三处一致，守卫测试覆盖且绿。
6. 每个 EF 模块独立 SQLite 文件；真实启动序列被集成测试覆盖。
7. 上手即用：clone → F5（或加“先起后端”说明）能跑通；依赖无漏洞。
8. 自造 MVVM，无第三方 MVVM 框架；密码 PBKDF2 加盐。

---

**版本** v1.0（2026-06-23）· 实现：codex · 审核：Claude
