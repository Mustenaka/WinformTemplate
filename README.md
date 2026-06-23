# WinformTemplate

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)

一个功能完善的 Windows Forms 应用程序参考架构模板，采用现代化设计模式和最佳实践。

[English](README_EN.md) | 简体中文

</div>

## 📋 目录

- [项目简介](#-项目简介)
- [核心特性](#-核心特性)
- [技术栈](#-技术栈)
- [快速开始](#-快速开始)
- [项目架构](#-项目架构)
- [配置说明](#-配置说明)
- [开发指南](#-开发指南)
- [数据库设计](#-数据库设计)
- [贡献指南](#-贡献指南)
- [许可证](#-许可证)

## 🎯 项目简介

WinformTemplate 是一个基于 .NET 8.0 的 Windows Forms 企业级应用程序模板，集成了现代化的架构设计和常用功能模块。本项目旨在为 WinForms 开发者提供一个开箱即用的参考架构，遵循 SOLID 原则和企业级开发规范。

### 适用场景

- 企业管理系统（ERP、CRM、MES 等）
- 桌面工具应用开发
- 数据采集与分析平台
- 系统集成应用
- WinForms 项目的二次开发基础

## ✨ 核心特性

### 架构特性

- **🏗️ 分层架构** - UI层、业务层、数据层清晰分离
- **🎨 MVVM 模式** - 完整的 Model-View-ViewModel 实现
- **📦 Repository 模式** - 统一的数据访问接口
- **💉 依赖注入** - 支持 DI 容器和服务管理
- **🔄 异步编程** - 全面的异步/等待支持

### 功能特性

- **🎯 现代化UI** - 集成 AntdUI 美观控件库
- **📊 数据可视化** - LiveCharts 图表支持
- **📝 日志系统** - log4net 四级日志（Info/Warn/Error/Fatal）
- **🗄️ ORM 支持** - Entity Framework Core 8.0
- **🔐 加密工具** - DES/AES/RSA 加密算法
- **📄 文件操作** - Excel (NPOI)、CSV (CsvHelper) 支持
- **⚙️ 配置管理** - JSON/XML/INI 配置文件加载
- **🧪 单元测试** - NUnit 测试框架

## 🛠️ 技术栈

### 核心框架

| 技术 | 版本 | 说明 |
|------|------|------|
| .NET | 8.0 | 目标框架 |
| Windows Forms | net8.0-windows | UI 框架 |
| C# | 12.0 | 编程语言 |

### 第三方库

| 包名 | 版本 | 用途 |
|------|------|------|
| [AntdUI](https://github.com/AntdUI/AntdUI) | 2.2.3 | 现代化 UI 控件库 |
| [Entity Framework Core](https://docs.microsoft.com/ef/core/) | 8.0.13 | ORM 数据访问框架 |
| [MySqlConnector](https://mysqlconnector.net/) | 2.4.0 | MySQL 数据库连接器 |
| [log4net](https://logging.apache.org/log4net/) | 3.2.0 | 日志记录框架 |
| [NPOI](https://github.com/nissl-lab/npoi) | 2.7.5 | Excel 文件处理 |
| [CsvHelper](https://joshclose.github.io/CsvHelper/) | 33.1.0 | CSV 文件处理 |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | 13.0.4 | JSON 序列化 |
| [LiveCharts](https://livecharts.dev/) | 0.9.8 | 数据可视化图表 |
| [NUnit](https://nunit.org/) | 3.14.0 | 单元测试框架 |

## 🚀 快速开始

### 前置要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (推荐) 或 Visual Studio Code
- [MySQL 8.0+](https://dev.mysql.com/downloads/mysql/) 数据库服务器

### 安装步骤

1. **克隆仓库**

```bash
git clone https://github.com/yourusername/WinformTemplate.git
cd WinformTemplate
```

2. **配置数据库**

编辑 `WinformTemplate/Resources/Config/config.json` 文件：

```json
{
  "DB": "server=127.0.0.1;port=3306;user=root;database=base;password=your_password;"
}
```

3. **初始化数据库**

项目首次运行会自动创建数据库表和初始化种子数据：
- 默认管理员账户：`admin`
- 默认密码：`123456`

4. **构建项目**

```bash
dotnet restore
dotnet build
```

5. **运行应用**

```bash
dotnet run --project WinformTemplate
```

或在 Visual Studio 中按 `F5` 启动调试。

## 📐 项目架构

本项目采用清晰的分层架构，详细架构说明请参阅 [ARCHITECTURE.md](ARCHITECTURE.md)。

### 目录结构

```
WinformTemplate/
├── Src/                          # 核心源代码
│   ├── Business/                 # 业务逻辑层
│   │   └── Sys/                 # 系统业务模块
│   │       ├── Context/         # EF Core 数据库上下文
│   │       ├── Model/           # 数据模型
│   │       ├── Repositories/    # 数据仓储
│   │       └── Service/         # 业务服务
│   ├── Common/                   # 公共模块
│   │   ├── MVVM/                # MVVM 框架
│   │   ├── Command/             # 命令模式
│   │   ├── Repository/          # 仓储基类
│   │   └── Patterns/            # 设计模式
│   ├── Config/                   # 配置管理
│   ├── FIO/                      # 文件 I/O
│   │   └── Excel/               # Excel 处理
│   ├── Logger/                   # 日志系统
│   ├── Tools/                    # 工具库
│   │   ├── Encryption/          # 加密工具
│   │   ├── Files/               # 文件操作
│   │   └── DataConvert/         # 数据转换
│   └── UIComponent/              # UI 组件
├── UI/                           # 用户界面层
│   ├── Activate/                # 激活窗体
│   ├── TestPage/                # 测试页面
│   └── Verify/                  # 验证页面
├── Resources/                    # 资源文件
│   ├── Config/                  # 配置文件
│   └── Log4net/                 # 日志配置
├── MainForm.cs                  # 主窗体
└── Program.cs                   # 应用入口
```

### 架构层次

```
┌─────────────────────────────────────┐
│         UI Layer (WinForms)         │  ← 用户界面层
├─────────────────────────────────────┤
│       ViewModel (MVVM Pattern)      │  ← 视图模型层
├─────────────────────────────────────┤
│      Service Layer (Business)       │  ← 业务逻辑层
├─────────────────────────────────────┤
│   Repository (Data Access Layer)    │  ← 数据访问层
├─────────────────────────────────────┤
│    DbContext (Entity Framework)     │  ← ORM 层
├─────────────────────────────────────┤
│         Database (MySQL)            │  ← 数据库层
└─────────────────────────────────────┘
```

详细的架构设计请查看 **[架构文档](ARCHITECTURE.md)**。

## ⚙️ 配置说明

### 数据库配置

文件位置：`Resources/Config/config.json`

```json
{
  "DB": "server=127.0.0.1;port=3306;user=root;database=base;password=123456;"
}
```

### 日志配置

文件位置：`Resources/Log4net/log4net.config`

支持四级日志：
- **Info** - 信息日志 (`Log/LogInfo/`)
- **Warn** - 警告日志 (`Log/LogWarn/`)
- **Error** - 错误日志 (`Log/LogError/`)
- **Fatal** - 致命错误日志 (`Log/LogFatal/`)

日志特性：
- 按日期自动分割 (yyyyMMdd 格式)
- HTML 格式输出
- 最大单文件 10MB
- 保留最近 100 个备份

### 使用日志

```csharp
using WinformTemplate.Logger;

// 记录信息日志
Debug.Info("应用程序启动成功");

// 记录警告
Debug.Warn("配置文件未找到，使用默认配置");

// 记录错误
Debug.Error("数据库连接失败", exception);

// 记录致命错误
Debug.Fatal("应用程序崩溃", exception);
```

## 👨‍💻 开发指南

### 添加新功能模块

1. **创建数据模型** (`Src/Business/YourModule/Model/`)
2. **定义 DbContext** (`Src/Business/YourModule/Context/`)
3. **实现 Repository** (`Src/Business/YourModule/Repositories/`)
4. **编写 Service** (`Src/Business/YourModule/Service/`)
5. **创建 ViewModel** (继承 `BaseViewModel`)
6. **设计 UI** (使用 AntdUI 控件)

### MVVM 绑定示例

```csharp
// ViewModel
public class UserViewModel : BaseViewModel
{
    private string _username;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public ICommand SaveCommand { get; }

    public UserViewModel()
    {
        SaveCommand = new RelayCommand(Save, CanSave);
    }

    private async void Save()
    {
        await ExecuteAsync(async () =>
        {
            // 保存逻辑
            await _repository.AddAsync(new User { Name = Username });
        });
    }

    private bool CanSave() => !string.IsNullOrEmpty(Username);
}

// View 绑定
var viewModel = new UserViewModel();
textBox.DataBindings.Add("Text", viewModel, nameof(viewModel.Username));
button.Click += (s, e) => viewModel.SaveCommand.Execute(null);
```

### 使用 Repository 模式

```csharp
// 1. 定义接口
public interface IUserRepository : IBaseRepository<UserModel>
{
    Task<UserModel> GetByUsernameAsync(string username);
}

// 2. 实现接口
public class UserRepository : BaseRepository<UserModel>, IUserRepository
{
    public UserRepository(YourDbContext context) : base(context) { }

    public async Task<UserModel> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }
}

// 3. 使用仓储
var user = await _userRepository.GetByUsernameAsync("admin");
var allUsers = await _userRepository.GetAllAsync();
```

### 快速与 AI 沟通

为了使 ChatGPT/Claude 等 AI 更好地理解本项目进行二次开发，可以使用以下提示词：

> 接下来我有一系列 C# WinForm 程序的开发问题。为了使你的回复更加准确，我使用如下技术栈：
> 1. .NET 8.0 Windows Forms 应用程序
> 2. AntdUI (2.2.3) - 现代化 UI 控件库
> 3. Entity Framework Core (8.0.13) - ORM 框架操作 MySQL
> 4. MySqlConnector (2.4.0) - MySQL 数据库连接
> 5. NPOI (2.7.5) - Excel 文件交互
> 6. CsvHelper (33.1.0) - CSV 文件交互
> 7. log4net (3.2.0) - 日志处理
> 8. Newtonsoft.Json (13.0.4) - JSON 序列化
> 9. LiveCharts (0.9.8) - 数据可视化图表
> 10. NUnit (3.14.0) - 单元测试框架
> 11. MVVM 设计模式 + Repository 模式 + 依赖注入
>
> 在你理解我使用的技术栈之后，请直接且准确地回复我的开发问题。

## 🗄️ 数据库设计

### 核心表结构

#### SysAccount (系统账户表)
| 字段 | 类型 | 说明 |
|------|------|------|
| SysId | int | 主键 (自增) |
| SysUuid | string | 外部 UUID |
| SysAccountName | string | 账户名 |
| SysPassword | string | 密码 (MD5) |
| SysNickname | string | 昵称 |
| SysLevel | int | 账户级别 (0=最高) |
| SysRoleId | int | 角色ID (外键) |
| SysStatus | int | 状态 (0=有效, 1=无效) |

#### SysRole (系统角色表)
| 字段 | 类型 | 说明 |
|------|------|------|
| SrId | int | 主键 (自增) |
| SrName | string | 角色名称 |
| SrEnName | string | 英文名称 |
| SrStatus | int | 状态 |

#### SysMenu (系统菜单表)
| 字段 | 类型 | 说明 |
|------|------|------|
| SmId | int | 主键 (自增) |
| SmParentId | int | 父级菜单ID |
| SmName | string | 菜单名称 |
| SmType | int | 类型 (0=菜单, 1=内容) |
| SmUrl | string | 链接地址 |
| SmLevel | int | 菜单级别 |

#### SysRoleAuth (角色权限关联表)
| 字段 | 类型 | 说明 |
|------|------|------|
| SraRoleId | int | 角色ID (复合主键) |
| SraMenuId | int | 菜单ID (复合主键) |

### 初始数据

项目首次运行会自动创建以下种子数据：
- 管理员角色 (Admin)
- 管理员账户 (admin/123456)
- 系统管理菜单结构
- 角色权限分配

## 🤝 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 代码规范

- 遵循 C# 编码规范和命名约定
- 使用有意义的变量和方法名
- 添加必要的注释和 XML 文档
- 确保所有单元测试通过
- 保持代码整洁和可维护性

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

## 📞 联系方式

- 作者: Mustenaka
- 项目地址: [https://github.com/yourusername/WinformTemplate](https://github.com/yourusername/WinformTemplate)
- 问题反馈: [Issues](https://github.com/yourusername/WinformTemplate/issues)

## 🙏 致谢

感谢以下开源项目：
- [AntdUI](https://github.com/AntdUI/AntdUI) - 提供美观的 UI 控件
- [Entity Framework Core](https://github.com/dotnet/efcore) - 强大的 ORM 框架
- [log4net](https://logging.apache.org/log4net/) - 可靠的日志框架
- [NPOI](https://github.com/nissl-lab/npoi) - Excel 文件处理

## Demo Accounts

Default seeded accounts for the current refactor:

- `admin / 123456`: administrator, can access `/sys/user` and `/sys/role`.
- `operator / 123456`: limited operator, can access `/sys/user` only.

---

<div align="center">
Made with ❤️ by Mustenaka
</div>
