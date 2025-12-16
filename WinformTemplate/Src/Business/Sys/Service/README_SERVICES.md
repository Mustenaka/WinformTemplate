# WinformTemplate 业务服务层说明文档

## 概述

本文档描述了 WinformTemplate 项目的业务服务层实现，包括账户管理、菜单管理、角色管理、参数管理和权限管理等核心功能。

## 已实现的服务

### 1. MD5Helper - MD5加密辅助类
**文件位置**: `WinformTemplate/Src/Tools/Encryption/MD5Helper.cs`

**功能**:
- MD5加密（32位小写）
- MD5加密（32位大写）
- MD5验证

**使用示例**:
```csharp
using WinformTemplate.Tools.Encryption;

// 加密密码
string password = "123456";
string hash = MD5Helper.Encrypt(password); // e10adc3949ba59abbe56e057f20f883e

// 验证密码
bool isValid = MD5Helper.Verify("123456", hash); // true
```

---

### 2. SysAccountService - 账户服务
**接口**: `ISysAccountService`
**实现**: `SysAccountService`

**功能**:
- 用户登录（密码MD5加密验证）
- CRUD操作（创建、读取、更新、删除账户）
- 密码修改
- 账户冻结/解冻
- 权限验证
- 获取账户菜单

**使用示例**:
```csharp
// 依赖注入
var accountService = serviceProvider.GetService<ISysAccountService>();

// 用户登录
var account = await accountService.LoginAsync("admin", "123456");
if (account != null)
{
    Console.WriteLine($"登录成功: {account.SysNickname}");
}

// 创建账户
var newAccount = new SysAccountModel
{
    SysAccountName = "user001",
    SysPassword = "123456", // 服务会自动MD5加密
    SysNickname = "测试用户",
    SysRoleId = 2,
    SysLevel = 1
};
await accountService.CreateAccountAsync(newAccount);

// 修改密码
await accountService.ChangePasswordAsync(accountId, "123456", "newpassword");

// 冻结账户
await accountService.FreezeAccountAsync(accountId);

// 验证权限
bool hasPermission = await accountService.HasPermissionAsync(accountId, menuId);
```

---

### 3. SysMenuService - 菜单服务
**接口**: `ISysMenuService`
**实现**: `SysMenuService`

**功能**:
- 获取所有菜单
- 获取菜单树（树形结构）
- 根据角色获取菜单
- 根据账户获取菜单
- CRUD操作
- 菜单冻结/解冻
- 获取子菜单

**使用示例**:
```csharp
var menuService = serviceProvider.GetService<ISysMenuService>();

// 获取菜单树
var menuTree = await menuService.GetMenuTreeAsync();

// 根据账户获取菜单
var userMenus = await menuService.GetMenusByAccountIdAsync(accountId);

// 创建菜单
var newMenu = new SysMenuModel
{
    SmParentId = 0, // 0表示根菜单
    SmName = "系统管理",
    SmEnName = "System",
    SmType = 0, // 0-菜单，1-内容
    SmUrl = "/system",
    SmTarget = "SystemForm",
    SmLevel = 0,
    SmSort = 1
};
await menuService.CreateMenuAsync(newMenu);

// 获取子菜单
var childMenus = await menuService.GetChildMenusAsync(parentMenuId);
```

---

### 4. SysRoleService - 角色服务
**接口**: `ISysRoleService`
**实现**: `SysRoleService`

**功能**:
- CRUD操作
- 分配菜单权限给角色
- 批量分配菜单权限
- 移除角色菜单权限
- 获取角色的所有权限
- 检查角色权限

**使用示例**:
```csharp
var roleService = serviceProvider.GetService<ISysRoleService>();

// 创建角色
var newRole = new SysRoleModel
{
    SrName = "操作员",
    SrEnName = "Operator",
    SrRemark = "系统操作员角色"
};
await roleService.CreateRoleAsync(newRole);

// 分配单个菜单权限
await roleService.AssignMenuToRoleAsync(roleId, menuId);

// 批量分配菜单权限
var menuIds = new List<long> { 1, 2, 3, 4, 5 };
await roleService.AssignMenusToRoleAsync(roleId, menuIds);

// 获取角色的所有菜单
var roleMenus = await roleService.GetRoleMenusAsync(roleId);

// 检查权限
bool hasPermission = await roleService.HasMenuPermissionAsync(roleId, menuId);
```

---

### 5. SysParamService - 参数服务
**接口**: `ISysParamService`
**实现**: `SysParamService`

**功能**:
- CRUD操作
- 根据键获取值
- 根据键设置值
- 批量设置参数
- 检查键是否存在

**使用示例**:
```csharp
var paramService = serviceProvider.GetService<ISysParamService>();

// 设置参数
await paramService.SetValueByKeyAsync("app_version", "1.0.0");

// 获取参数值
string version = await paramService.GetValueByKeyAsync("app_version");

// 批量设置参数
var params = new Dictionary<string, string>
{
    { "db_host", "localhost" },
    { "db_port", "3306" },
    { "db_name", "mydb" }
};
await paramService.BatchSetParamsAsync(params);

// 检查键是否存在
bool exists = await paramService.KeyExistsAsync("app_version");
```

---

### 6. PermissionService - 权限管理服务
**接口**: `IPermissionService`
**实现**: `PermissionService`

**功能**:
- 验证用户是否有指定菜单的权限
- 获取用户所有可访问的菜单
- 获取用户可访问的菜单树
- 菜单树过滤（根据权限）
- 检查账户有效性
- 获取角色权限菜单ID

**使用示例**:
```csharp
var permissionService = serviceProvider.GetService<IPermissionService>();

// 验证权限
bool hasPermission = await permissionService.HasPermissionAsync(accountId, menuId);

// 获取用户可访问的菜单
var accessibleMenus = await permissionService.GetAccessibleMenusAsync(accountId);

// 获取用户可访问的菜单树
var menuTree = await permissionService.GetAccessibleMenuTreeAsync(accountId);

// 检查账户是否有效
bool isValid = await permissionService.IsAccountValidAsync(accountId);

// 获取角色的所有菜单ID
var menuIds = await permissionService.GetRoleMenuIdsAsync(roleId);
```

---

## 权限验证逻辑

权限验证遵循以下层级关系：

```
账户 (SysAccountModel)
  └─> 角色 (SysRoleModel) [通过 SysRoleId 关联]
       └─> 角色权限 (SysRoleAuthModel) [角色与菜单的多对多关联]
            └─> 菜单 (SysMenuModel)
```

**验证流程**:
1. 检查账户是否存在且未冻结（SysStatus=false表示有效）
2. 获取账户的角色ID（SysRoleId）
3. 查询角色权限表（sys_role_auth）获取该角色的所有菜单
4. 过滤无效菜单（SysStatus=true表示无效）
5. 验证用户是否有指定菜单的访问权限

---

## 数据状态说明

### SysStatus 字段含义
- **false** = 有效/正常/启用
- **true** = 无效/冻结/禁用

### SmParentId 字段含义
- **0** = 根菜单（顶级菜单）
- **其他值** = 父菜单的ID

---

## 依赖注入配置示例

在 Program.cs 或 Startup.cs 中注册服务：

```csharp
services.AddScoped<ISysAccountRepository, SysAccountRepository>();
services.AddScoped<ISysRoleRepository, SysRoleRepository>();
services.AddScoped<ISysMenuRepository, SysMenuRepository>();
services.AddScoped<ISysParamRepository, SysParamRepository>();

services.AddScoped<ISysAccountService, SysAccountService>();
services.AddScoped<ISysMenuService, SysMenuService>();
services.AddScoped<ISysRoleService, SysRoleService>();
services.AddScoped<ISysParamService, SysParamService>();
services.AddScoped<IPermissionService, PermissionService>();
```

---

## 日志记录

所有服务都使用 `WinformTemplate.Logger.Debug` 进行日志记录：

- **Debug.Info()** - 记录关键操作信息
- **Debug.Warn()** - 记录警告信息（如操作失败）
- **Debug.Error()** - 记录错误信息（如异常）

**日志示例**:
```
[INFO] 用户登录成功 - 用户：admin，角色ID：1
[WARN] 登录失败：密码错误 - admin
[ERROR] 获取账户菜单异常：连接超时
```

---

## 注意事项

1. **密码处理**: 所有密码在存储和验证时都使用MD5加密，例如 "123456" 的MD5值为 "e10adc3949ba59abbe56e057f20f883e"

2. **异步编程**: 所有服务方法都使用 async/await 模式，确保异步操作正确执行

3. **异常处理**: 所有方法都包含 try-catch 异常处理，记录错误日志后重新抛出异常

4. **数据验证**: 在执行操作前都会验证数据的有效性（如检查记录是否存在）

5. **时间戳**: 创建和更新操作会自动设置 CreateAt 和 UpdateAt 时间戳

6. **菜单树构建**: 菜单树会自动包含父菜单路径，即使父菜单不在直接权限列表中

---

## 完成情况

- [x] MD5Helper 加密辅助类
- [x] ISysAccountService 接口和 SysAccountService 实现
- [x] ISysMenuService 接口和 SysMenuService 实现
- [x] ISysRoleService 接口和 SysRoleService 实现
- [x] ISysParamService 接口和 SysParamService 实现
- [x] IPermissionService 接口和 PermissionService 实现

所有服务均已实现并包含完整的 XML 注释、日志记录和异常处理。
