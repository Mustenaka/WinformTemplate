---
name: winform-template-dev
description: Use when adding or changing modules in the WinformTemplate WinForms project, switching EF/WebApi/Local data sources, wiring a backend endpoint, adding menu permissions, or extending DemoNote/Product-style CRUD pages.
---

# WinformTemplate Development Skill

Use this skill for second-development tasks in the `WinformTemplate` client and its Phase II demo/backend integration.

## Project Positioning

This repository is a .NET 8 WinForms template using AntdUI, a homegrown MVVM layer, and pluggable module data sources. The intended layering is:

```text
UI/Form/UserControl -> ViewModel -> Service -> IRepository -> Ef/WebApi/Local implementation
```

The backend demo is a separate project and separate git repository:

```text
D:\Work\Code\CSharp\WinformTemplateServer
```

The client/backend REST contract source of truth is:

```text
docs/api-contract.md
```

## Directory Conventions

For a module named `Order`, follow this shape:

```text
WinformTemplate/Src/Business/Order/
  Model/
  Context/                  # only if the module has EF storage
  Repositories/
    EfCore/
    WebApi/
    Local/
  Service/
  ViewModel/

WinformTemplate/UI/Business/Order/
WinformTemplate/Resources/MockData/
WinformTemplate.Tests/Business/Order/
```

Existing examples:

- Product: configurable business module under `Src/Business/Template` and `UI/Business/Template/Product`.
- DemoNote: three fixed data-source pages under `Src/Business/Demo` and `UI/Business/Demo`.
- Account management: Sys module plus pagination and permission routing.

## Add A Module In 6 Steps

1. **Model**
   - Add the entity under `Src/Business/<Module>/Model`.
   - Use EF attributes for table and column names if EF is supported.
   - Keep display-only helpers in the model or ViewModel, not scattered in UI code.

2. **Repository contract**
   - Add `I<Module>Repository : IRepository<TEntity>`.
   - Add named query methods for module-specific search, for example `SearchByTitleAsync`.
   - Do not expose `IQueryable` or `Expression<Func<...>>`.

3. **Three repository implementations**
   - EF: inherit `EfRepositoryBase<TEntity>`.
   - WebApi: inherit `ApiRepositoryBase<TEntity>`.
   - Local: inherit `LocalRepositoryBase<TEntity>`.
   - Register configurable modules with `AddModuleRepository<IRepo, EfRepo, ApiRepo, LocalRepo>("<Module>")`.

4. **Service**
   - Add `I<Module>Service` and `<Module>Service` when the module is a normal business feature.
   - Service depends only on repository interfaces.
   - Push paging/search/sort/filter into repository calls.

5. **ViewModel**
   - Track `CurrentPage`, `PageSize`, `TotalCount`, `TotalPages`, search text, sort state, selected row, and busy/status state.
   - Expose async methods such as `LoadDataAsync`, `SearchAsync`, `GoToPageAsync`, and CRUD methods.
   - Let `DataSourceUnavailableException` produce a user-visible disconnected status.

6. **UI, DI, menu, tests**
   - Add a `UserControl` under `UI/Business/<Module>`.
   - Use `Dock`, `Anchor`, `TableLayoutPanel`, and `FlowLayoutPanel`; do not depend on real size during construction.
   - Register ViewModel, Service, repositories, and page in `AppServiceRegistration` and `PageRegistryDefaultPages`.
   - Add EF and Local menu seeds and role auths.
   - Add focused tests before finishing.

## Data Source Choices

### EF

Use EF for real local persistence. Each EF module must have its own `DbContext`, `DbContextService`, `IDatabaseInitializer`, and SQLite file.

Default SQLite files:

```text
Resources/Database/sys.db
Resources/Database/template.db
Resources/Database/demo.db
```

Reason: EF Core `EnsureCreated` does not safely support multiple `DbContext` types sharing one database. The first context can create the database and cause later contexts to skip table creation. New EF modules must be covered by `AppStartupIntegrationTests`.

### WebApi

Use WebApi when the data lives behind REST. Update `docs/api-contract.md` before changing the client or backend.

Client rules:

- Inherit `ApiRepositoryBase<TEntity>`.
- Map endpoints exactly to `docs/api-contract.md`.
- Use `ApiResponse<T>` envelope semantics.
- Transport failure must become `DataSourceUnavailableException`.

Backend rules:

- Implement in `D:\Work\Code\CSharp\WinformTemplateServer`.
- Keep .NET 8.
- Default local addresses: `https://localhost:5001` and `http://localhost:5000`.
- Add endpoint integration tests with `WebApplicationFactory`.

### Local

Use Local for resettable demo data.

- Seed file goes under `WinformTemplate/Resources/MockData/*.json`.
- Inherit `LocalRepositoryBase<TEntity>`.
- CRUD stays in memory and does not write back to disk.
- Tests that mutate Local data should use a temporary seed root.

## Phase II Red Lines

- No `DbContext`, `HttpClient`, EF types, or `Expression<Func<...>>` in Service, ViewModel, or UI.
- No `GetAll().Where(...)` outside repositories.
- Client and backend share one REST contract: `docs/api-contract.md`.
- EF and Local pages must keep working when the backend is off.
- WebApi pages must degrade clearly when the backend is unavailable.
- Menu keys must match in EF seed, Local seed, and `PageRegistryDefaultPages`.
- Each EF module uses an independent SQLite file.
- Use homegrown MVVM only; do not add a third-party MVVM framework.
- Passwords use PBKDF2 through `PasswordHasher`.
- Runtime SQLite files are ignored; do not commit `*.db`, `*.db-shm`, or `*.db-wal`.

## Required Tests

Always run:

```powershell
dotnet build WinformTemplate.sln -warnaserror
dotnet test WinformTemplate.sln
dotnet list WinformTemplate.sln package --vulnerable --include-transitive
```

When changing menus:

- `SysMenuSeedUrls_AreConsistentAcrossEfLocalAndRegisteredPages`

When adding EF modules:

- Extend `AppStartupIntegrationTests` to verify the new module database file and a real query.

When adding or changing registered pages:

- `PageConstructionSmokeTests` must still construct every default page on STA without rendering.

When changing WebApi:

- Add fake-client mapping tests in `ApiRepositoryTests`.
- Add or update backend `WebApplicationFactory` tests in `WinformTemplateServer`.

## Implementation Bias

Prefer copying the closest existing pattern:

- Product for normal configurable modules.
- DemoNote for EF/WebAPI/Local side-by-side teaching pages.
- Account page for paged Sys UI and page construction safety.
- `docs/二开指南.md` for the full second-development walkthrough.
