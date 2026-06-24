---
name: winformtemplate-business-modifier
description: Modify the WinformTemplate .NET 8 WinForms client and optional WinformTemplateServer backend according to this repository's architecture. Use for second-development or vibe-coding tasks such as adding business modules, changing fields, filters, menus, permissions, EF/WebAPI/Local repositories, REST contracts, Minimal API endpoints, ViewModels, WinForms AntdUI pages, and tests.
---

# WinformTemplate Business Modifier

Use this skill to implement business changes in:

- Client: `D:\Work\Code\CSharp\WinformTemplate`
- Server: `D:\Work\Code\CSharp\WinformTemplateServer`

## Load Context First

Before editing, read the relevant files:

- Always read `README.md`, `docs/项目架构与文件结构.md`, `docs/二开指南.md`, and `docs/api-contract.md`.
- For service/API work, also read `D:\Work\Code\CSharp\WinformTemplateServer\README.md` and `src/WinformTemplateServer/Program.cs`.
- For UI or workflow changes, inspect the closest existing page before coding: Product for normal modules, DemoNote for data-source comparisons, Sys pages for account/role/permission behavior.

## Core Architecture Rules

- Keep the normal chain as `UI -> ViewModel -> Service -> Repository`.
- Do not access `DbContext`, `HttpClient`, or repository implementations from UI, ViewModel, or Service.
- Push paging, keyword search, sorting, and filters into Repository methods.
- Do not expose `IQueryable` or `Expression<Func<...>>` from Repository interfaces.
- Use the existing MVVM primitives: `ObservableObject`, `BaseViewModel`, `RelayCommand`, and binding extensions.
- Keep WebAPI transport failures as `DataSourceUnavailableException`; do not return empty data to fake success.
- Keep passwords on `PasswordHasher` PBKDF2; do not introduce plaintext or weak hashes.

## Choose The Pattern

- Normal business module: follow Product.
  - `Src/Business/Template/Model/ProductModel.cs`
  - `IProductRepository`
  - `EfProductRepository`, `ApiProductRepository`, `LocalProductRepository`
  - `ProductService`
  - `ProductManagementViewModel`
  - `ProductManagementControl`
- Data-source comparison/demo: follow DemoNote.
  - One entity and ViewModel
  - Three pages fixed to EF, WebAPI, and Local repositories
- Account, role, menu, permission work: follow Sys.
  - Keep `SysDatabaseInitializer`, `Resources/MockData/sysMenus.json`, `Resources/MockData/sysRoleAuths.json`, and `PageRegistryDefaultPages` synchronized.

## New Module Checklist

For a normal module, create or update:

1. Model under `WinformTemplate/Src/Business/{Module}/Model`.
2. Repository interface under `Repositories`.
3. EF repository under `Repositories/EfCore`.
4. WebAPI repository under `Repositories/WebApi`.
5. Local repository and MockData JSON under `Repositories/Local` and `Resources/MockData`.
6. Service and interface when the module is not a Demo-style direct repository page.
7. ViewModel with paging, search, sorting, CRUD state, busy state, and readable status messages.
8. UserControl under `WinformTemplate/UI/Business/{Module}` using existing WinForms/AntdUI patterns.
9. DI registrations in `AppServiceRegistration`.
10. Page registration in `PageRegistryDefaultPages`.
11. EF menu seed, Local menu seed, and role-auth seed when a new menu is added.
12. Tests for repository behavior, ViewModel behavior, navigation/menu consistency, and startup if a new EF module is added.

## WebAPI And Server Work

Use `docs/api-contract.md` as the source of truth.

When adding or changing API behavior:

1. Update `docs/api-contract.md` first.
2. Update or add the client `ApiXxxRepository`.
3. Implement matching server endpoints in `D:\Work\Code\CSharp\WinformTemplateServer`.
4. Keep all responses wrapped in `ApiResponse<T>`.
5. Return business failures with `success=false` and `isTransportError=false`.
6. Add server integration tests with temporary SQLite files.

The current server is Minimal API in `src/WinformTemplateServer/Program.cs`. If endpoint count grows, prefer module endpoint registration rather than making `Program.cs` unbounded.

## UI Rules

- Reuse existing AntdUI control patterns from Product, DemoNote, Account, and Role pages.
- Prefer layout containers such as `Dock`, `Anchor`, `TableLayoutPanel`, and `FlowLayoutPanel`.
- Do not rely on real control size during constructor execution.
- If using `SplitContainer`, defer risky `SplitterDistance` and min-size logic until `HandleCreated`, `Load`, or `SizeChanged`.
- Keep page text operational and concise.

## Validation

Run the smallest useful test set, then broaden by risk:

```powershell
dotnet test WinformTemplate.sln
```

For server changes:

```powershell
cd D:\Work\Code\CSharp\WinformTemplateServer
dotnet test WinformTemplateServer.sln
```

Use these targeted guards when relevant:

- Navigation/menu changes: `NavigationPermissionTests`, `PageConstructionSmokeTests`.
- EF startup or new DbContext: `AppStartupIntegrationTests`.
- Repository changes: matching repository tests and `ApiRepositoryTests`.
- ViewModel changes: matching ViewModel tests.

## Response Style

When done, report:

- Changed files.
- What behavior was added or modified.
- Tests run and results.
- Any intentionally deferred scope, especially unimplemented server endpoints or UI limitations.
