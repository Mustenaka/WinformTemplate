# Sys Service Notes

Sys 服务层只依赖仓储契约，不直接引用 EF、DbContext、HttpClient 或 `Expression<Func<...>>`。

## 主要服务

- `SysAccountService`：登录、账户 CRUD、冻结/解冻、权限辅助查询。
- `SysRoleService`：角色管理。
- `SysMenuService`：菜单管理。
- `SysParamService`：系统参数管理。
- `PermissionService`：按角色过滤菜单、判断菜单权限。

## 密码

密码通过 `PasswordHasher` 使用 PBKDF2 加盐哈希。默认演示账号：

- `admin / 123456`
- `operator / 123456`

旧哈希兼容迁移由 `Security.UpgradeLegacyPasswordHashOnLogin` 控制。

## 查询

登录链路使用 `ISysAccountRepository.GetByUsernameAsync` 下推查询，不做全表扫描。

账户列表已提供 `QueryAccountsAsync(keyword, page, pageSize)` 分页入口。当前账户管理 UI 仍是简单列表，后续可参考 Product 页补分页控件。

## 数据源

Sys 仓储按模块数据源切换：

- `EfSys*Repository`
- `ApiSys*Repository`
- `LocalSys*Repository`

切换由 `config.json -> DataSource.Modules.Sys` 控制。
