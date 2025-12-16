# WinformTemplate 架构设计文档

## 📑 目录

- [1. 架构概述](#1-架构概述)
- [2. 分层架构设计](#2-分层架构设计)
- [3. 设计模式](#3-设计模式)
- [4. 数据访问层](#4-数据访问层)
- [5. 业务逻辑层](#5-业务逻辑层)
- [6. 用户界面层](#6-用户界面层)
- [7. 公共基础设施](#7-公共基础设施)
- [8. 配置与日志](#8-配置与日志)
- [9. 依赖注入](#9-依赖注入)
- [10. 数据流向](#10-数据流向)
- [11. 最佳实践](#11-最佳实践)

---

## 1. 架构概述

### 1.1 架构原则

WinformTemplate 遵循以下核心架构原则：

- **关注点分离 (Separation of Concerns)** - 每一层只关注自己的职责
- **单一职责原则 (Single Responsibility Principle)** - 每个类只有一个改变的理由
- **依赖倒置原则 (Dependency Inversion Principle)** - 高层模块不依赖低层模块，都依赖抽象
- **开闭原则 (Open/Closed Principle)** - 对扩展开放，对修改关闭
- **接口隔离原则 (Interface Segregation Principle)** - 使用多个专门的接口优于使用单一的总接口

### 1.2 架构图

```
┌─────────────────────────────────────────────────────────────┐
│                        用户界面层 (UI)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  MainForm    │  │  Activate    │  │  TestPage    │      │
│  │              │  │  Form        │  │  Controls    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────────┬────────────────────────────────┘
                             │ Data Binding
┌────────────────────────────▼────────────────────────────────┐
│                    视图模型层 (ViewModel)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │BaseViewModel │  │ RelayCommand │  │ Observable   │      │
│  │              │  │              │  │ Object       │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────────┬────────────────────────────────┘
                             │ Business Logic
┌────────────────────────────▼────────────────────────────────┐
│                      业务逻辑层 (Service)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │SysDbContext  │  │  Business    │  │  Validation  │      │
│  │  Service     │  │  Logic       │  │  Rules       │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────────┬────────────────────────────────┘
                             │ Data Access
┌────────────────────────────▼────────────────────────────────┐
│                    数据访问层 (Repository)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │BaseRepository│  │ IRepository  │  │  CRUD Ops    │      │
│  │              │  │ Interface    │  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────────┬────────────────────────────────┘
                             │ ORM Mapping
┌────────────────────────────▼────────────────────────────────┐
│                     ORM 层 (DbContext)                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  SysDbContext│  │  Entity      │  │  Migration   │      │
│  │              │  │  Configuration│  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────────┬────────────────────────────────┘
                             │ SQL Commands
┌────────────────────────────▼────────────────────────────────┐
│                      数据库层 (MySQL)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  SysAccount  │  │   SysRole    │  │   SysMenu    │      │
│  │    Table     │  │    Table     │  │    Table     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

### 1.3 技术栈选择理由

| 技术 | 选择理由 |
|------|----------|
| .NET 8.0 | 最新的长期支持版本，性能优异，跨平台支持 |
| WinForms | 成熟稳定的桌面应用框架，丰富的控件生态 |
| AntdUI | 现代化设计语言，美观的 UI 组件，易用性强 |
| EF Core | 强大的 ORM 框架，代码优先，支持迁移管理 |
| MySQL | 开源免费，性能可靠，广泛应用于企业级系统 |
| log4net | 成熟的日志框架，灵活的配置，高性能输出 |

---

## 2. 分层架构设计

### 2.1 层次划分

#### 2.1.1 用户界面层 (UI Layer)

**位置**: `WinformTemplate/UI/` 和根目录的 `MainForm.cs`

**职责**:
- 用户交互界面展示
- 用户输入事件处理
- 数据绑定到 ViewModel
- UI 控件状态管理

**主要组件**:
```
UI/
├── MainForm.cs                # 主窗体
├── Activate/                  # 激活模块
│   └── ActivateForm.cs
├── TestPage/                  # 测试页面
│   ├── UCPageTestPage1.cs
│   └── UCPageTestPage2.cs
└── Verify/                    # 验证模块
    └── UCPage_Verify.cs
```

**设计原则**:
- UI 层不应包含业务逻辑
- 通过数据绑定与 ViewModel 通信
- 使用 UserControl 实现界面模块化
- 保持代码简洁，复杂逻辑下沉到 ViewModel

#### 2.1.2 视图模型层 (ViewModel Layer)

**位置**: `WinformTemplate/Src/Common/MVVM/`

**职责**:
- 作为 View 和 Model 之间的桥梁
- 处理 UI 逻辑和状态管理
- 实现数据验证
- 提供命令绑定

**核心组件**:

```csharp
// 基类：ObservableObject
public class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// ViewModel 基类
public class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    protected async Task ExecuteAsync(Func<Task> operation)
    {
        try
        {
            IsBusy = true;
            await operation();
        }
        catch (Exception ex)
        {
            StatusMessage = $"错误: {ex.Message}";
            Debug.Error("ViewModel 操作失败", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

**绑定扩展**:
- `AntdUIBindingExtensions.cs` - AntdUI 控件的数据绑定扩展
- `DefaultBindingExtensions.cs` - 标准 WinForms 控件的绑定扩展

#### 2.1.3 业务逻辑层 (Service Layer)

**位置**: `WinformTemplate/Src/Business/Sys/Service/`

**职责**:
- 实现业务规则和业务流程
- 协调多个 Repository 操作
- 处理事务管理
- 数据验证和业务逻辑验证

**示例 - SysDbContextService**:

```csharp
public class SysDbContextService
{
    private readonly IServiceProvider _serviceProvider;

    public SysDbContextService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // 确保数据库创建
    public async Task EnsureCreatedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SysDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    // 初始化数据库（种子数据）
    public async Task InitializeDatabaseAsync()
    {
        // 1. 创建角色
        // 2. 创建账户
        // 3. 创建菜单
        // 4. 分配权限
    }

    // 事务管理
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SysDbContext>();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

#### 2.1.4 数据访问层 (Repository Layer)

**位置**: `WinformTemplate/Src/Business/Sys/Repositories/` 和 `Src/Common/Repository/`

**职责**:
- 封装数据库操作
- 提供 CRUD 接口
- 实现查询逻辑
- 处理数据映射

**架构设计**:

```csharp
// 仓储接口
public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(object id);
    Task<IEnumerable<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate);
    Task<(IEnumerable<TEntity> items, int total)> GetPagedAsync(int page, int pageSize);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateAsync(TEntity entity);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteAsync(TEntity entity);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
    Task<int> SaveChangesAsync();
}

// 仓储基类实现
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<TEntity> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetByConditionAsync(
        Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<(IEnumerable<TEntity> items, int total)> GetPagedAsync(
        int page, int pageSize)
    {
        var total = await _dbSet.CountAsync();
        var items = await _dbSet
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    // ... 其他 CRUD 实现
}
```

**具体实现示例**:

```csharp
public interface ISysAccountRepository : IBaseRepository<SysAccountModel>
{
    Task<SysAccountModel> GetByUsernameAsync(string username);
    Task<bool> FreezeAccountAsync(int accountId);
    Task<bool> UnfreezeAccountAsync(int accountId);
}

public class SysAccountRepository : BaseRepository<SysAccountModel>, ISysAccountRepository
{
    public SysAccountRepository(SysDbContext context) : base(context) { }

    public async Task<SysAccountModel> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(a => a.Role)
            .Include(a => a.Extend)
            .FirstOrDefaultAsync(a => a.SysAccountName == username);
    }

    public async Task<bool> FreezeAccountAsync(int accountId)
    {
        var account = await GetByIdAsync(accountId);
        if (account == null) return false;

        account.SysStatus = 1;
        account.SysUpdateAt = DateTime.Now;
        await UpdateAsync(account);
        return true;
    }
}
```

#### 2.1.5 数据模型层 (Model Layer)

**位置**: `WinformTemplate/Src/Business/Sys/Model/`

**职责**:
- 定义数据实体
- 映射数据库表结构
- 定义实体关系
- 数据验证规则

**示例 - SysAccountModel**:

```csharp
[Table("SysAccount")]
public class SysAccountModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SysId { get; set; }

    [Required]
    [MaxLength(64)]
    public string SysUuid { get; set; }

    [Required]
    [MaxLength(128)]
    public string SysAccountName { get; set; }

    [Required]
    [MaxLength(256)]
    public string SysPassword { get; set; }  // MD5 加密

    [MaxLength(128)]
    public string SysNickname { get; set; }

    public int SysLevel { get; set; } = 999;

    public int SysRoleId { get; set; }

    [ForeignKey("SysRoleId")]
    public SysRoleModel Role { get; set; }

    public int? SysExtendId { get; set; }

    [ForeignKey("SysExtendId")]
    public SysExtendModel Extend { get; set; }

    public int SysStatus { get; set; } = 0;  // 0=有效, 1=无效

    public DateTime SysCreateAt { get; set; } = DateTime.Now;

    public DateTime SysUpdateAt { get; set; } = DateTime.Now;

    [MaxLength(256)]
    public string SysReserved1 { get; set; }

    [MaxLength(256)]
    public string SysReserved2 { get; set; }

    [MaxLength(256)]
    public string SysReserved3 { get; set; }
}
```

#### 2.1.6 数据库上下文层 (DbContext Layer)

**位置**: `WinformTemplate/Src/Business/Sys/Context/Full/`

**职责**:
- 配置 EF Core
- 定义 DbSet
- 配置实体关系
- 数据库连接管理

**示例 - SysDbContext**:

```csharp
public class SysDbContext : DbContext
{
    public DbSet<SysAccountModel> SysAccounts { get; set; }
    public DbSet<SysMenuModel> SysMenus { get; set; }
    public DbSet<SysParamModel> SysParams { get; set; }
    public DbSet<SysRoleModel> SysRoles { get; set; }
    public DbSet<SysRoleAuthModel> SysRoleAuths { get; set; }
    public DbSet<SysExtendModel> SysExtends { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = GlobalProjectConfig.Instance.DbConfig;

            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 21)),
                options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                    options.CommandTimeout(60);
                });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置复合主键
        modelBuilder.Entity<SysRoleAuthModel>()
            .HasKey(ra => new { ra.SraRoleId, ra.SraMenuId });

        // 配置级联删除
        modelBuilder.Entity<SysRoleAuthModel>()
            .HasOne(ra => ra.Role)
            .WithMany(r => r.RoleAuths)
            .HasForeignKey(ra => ra.SraRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SysRoleAuthModel>()
            .HasOne(ra => ra.Menu)
            .WithMany(m => m.RoleAuths)
            .HasForeignKey(ra => ra.SraMenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## 3. 设计模式

### 3.1 MVVM 模式 (Model-View-ViewModel)

**实现位置**: `Src/Common/MVVM/`

**组件说明**:

```
MVVM 模式实现
│
├── Model (数据模型)
│   └── BaseModel.cs
│       └── 实现 INotifyPropertyChanged
│
├── View (用户界面)
│   └── MainForm.cs / UserControls
│       └── 数据绑定到 ViewModel
│
├── ViewModel (视图模型)
│   ├── ObservableObject.cs
│   ├── BaseViewModel.cs
│   └── 继承自定义 ViewModel
│
└── Command (命令)
    └── RelayCommand.cs
        ├── RelayCommand (无参数)
        └── RelayCommand<T> (带参数)
```

**数据绑定示例**:

```csharp
// ViewModel
public class LoginViewModel : BaseViewModel
{
    private string _username;
    private string _password;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
    }

    private async void ExecuteLogin()
    {
        await ExecuteAsync(async () =>
        {
            var account = await _accountRepository.GetByUsernameAsync(Username);
            if (account != null && account.SysPassword == MD5Helper.Hash(Password))
            {
                StatusMessage = "登录成功";
                // 导航到主界面
            }
            else
            {
                StatusMessage = "用户名或密码错误";
            }
        });
    }

    private bool CanExecuteLogin()
    {
        return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
    }
}

// View 绑定
public partial class LoginForm : Form
{
    private readonly LoginViewModel _viewModel;

    public LoginForm()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        SetupBindings();
    }

    private void SetupBindings()
    {
        // 绑定文本框
        txtUsername.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.Username));
        txtPassword.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.Password));

        // 绑定状态消息
        lblStatus.DataBindings.Add(nameof(Label.Text), _viewModel, nameof(_viewModel.StatusMessage));

        // 绑定按钮命令
        btnLogin.Click += (s, e) => _viewModel.LoginCommand.Execute(null);
    }
}
```

### 3.2 Repository 模式

**实现位置**: `Src/Common/Repository/` 和 `Src/Business/Sys/Repositories/`

**优势**:
- 将数据访问逻辑与业务逻辑分离
- 提供统一的数据操作接口
- 便于单元测试（可 Mock）
- 支持缓存和查询优化

**实现层次**:

```
Repository 模式层次
│
├── IBaseRepository<TEntity>          # 通用仓储接口
│   ├── GetAllAsync()
│   ├── GetByIdAsync()
│   ├── AddAsync()
│   ├── UpdateAsync()
│   └── DeleteAsync()
│
├── BaseRepository<TEntity>           # 通用仓储实现
│   └── 实现 IBaseRepository 接口
│
├── ISysAccountRepository             # 特定仓储接口
│   ├── 继承 IBaseRepository
│   └── GetByUsernameAsync()          # 自定义方法
│
└── SysAccountRepository              # 特定仓储实现
    ├── 继承 BaseRepository
    └── 实现 ISysAccountRepository
```

### 3.3 单例模式 (Singleton)

**实现位置**: `Src/Common/Patterns/Singleton.cs`

**应用场景**: 全局配置、日志管理、数据库连接池

**实现方式**:

```csharp
public abstract class SingletonBase<T> where T : SingletonBase<T>, new()
{
    private static readonly Lazy<T> _instance = new Lazy<T>(() => new T(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static T Instance => _instance.Value;

    protected SingletonBase()
    {
        // 防止外部实例化
    }
}

// 使用示例
public class GlobalProjectConfig : SingletonBase<GlobalProjectConfig>
{
    public string DbConfig { get; private set; }
    public string AppVersion { get; private set; }
    public string AppName { get; private set; }

    public GlobalProjectConfig()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Config", "config.json");
        var json = File.ReadAllText(configPath);
        var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        DbConfig = config["DB"];
    }
}

// 使用
var dbConfig = GlobalProjectConfig.Instance.DbConfig;
```

### 3.4 命令模式 (Command Pattern)

**实现位置**: `Src/Common/MVVM/Command/RelayCommand.cs`

**优势**:
- 解耦调用者和接收者
- 支持撤销/重做操作
- 可组合多个命令
- 便于参数化操作

**实现**:

```csharp
public interface IRelayCommand : ICommand
{
    void RaiseCanExecuteChanged();
}

public class RelayCommand : IRelayCommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public event EventHandler CanExecuteChanged;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute();
    }

    public void Execute(object parameter)
    {
        _execute();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

// 带参数版本
public class RelayCommand<T> : IRelayCommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    public event EventHandler CanExecuteChanged;

    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute((T)parameter);
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

### 3.5 抽象工厂模式 (Abstract Factory)

**实现位置**: `Src/Common/Patterns/AbstractFactory.cs`

**应用场景**: 创建不同类型的对象实例，如不同数据库的上下文、不同的导出器等

**实现**:

```csharp
public abstract class AbstractFactory<TProduct>
{
    private readonly Dictionary<string, Func<TProduct>> _factories = new();

    public void Register(string key, Func<TProduct> factory)
    {
        if (!_factories.ContainsKey(key))
        {
            _factories.Add(key, factory);
        }
    }

    public TProduct Create(string key)
    {
        if (_factories.TryGetValue(key, out var factory))
        {
            return factory();
        }
        throw new ArgumentException($"未找到类型为 {key} 的工厂方法");
    }

    public bool IsRegistered(string key)
    {
        return _factories.ContainsKey(key);
    }
}

// 使用示例：导出器工厂
public class ExporterFactory : AbstractFactory<IExporter>
{
    public static readonly ExporterFactory Instance = new();

    private ExporterFactory()
    {
        Register("Excel", () => new ExcelExporter());
        Register("CSV", () => new CsvExporter());
        Register("PDF", () => new PdfExporter());
    }
}

// 使用
var exporter = ExporterFactory.Instance.Create("Excel");
exporter.Export(data);
```

---

## 4. 数据访问层

### 4.1 Entity Framework Core 配置

**版本**: EF Core 8.0.13

**数据库提供商**: MySQL (MySqlConnector 2.4.0)

**配置策略**:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        var connectionString = GlobalProjectConfig.Instance.DbConfig;

        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 21)),
            mySqlOptions =>
            {
                // 启用连接重试
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null
                );

                // 命令超时
                mySqlOptions.CommandTimeout(60);

                // 启用详细错误
                mySqlOptions.EnableDetailedErrors();
            })
            .EnableSensitiveDataLogging()  // 开发环境
            .EnableDetailedErrors();       // 开发环境
    }
}
```

### 4.2 数据模型设计

**命名规范**:
- 表名: `Sys` + 模块名（如 SysAccount）
- 字段名: 表名缩写 + 字段名（如 SysId, SysAccountName）
- 外键: 外表名缩写 + Id（如 SysRoleId）

**字段约定**:
- 主键: `[Table]Id` (int, 自增)
- 状态字段: `[Table]Status` (0=有效, 1=无效)
- 时间戳: `[Table]CreateAt`, `[Table]UpdateAt`
- 保留字段: `[Table]Reserved1/2/3` (预留扩展)

**实体关系图**:

```
┌─────────────────┐         ┌─────────────────┐
│  SysAccount     │ N     1 │    SysRole      │
│  - SysId (PK)   ├─────────┤  - SrId (PK)    │
│  - SysRoleId(FK)│         │  - SrName       │
│  - SysAccountName│         │  - SrEnName     │
│  - SysPassword  │         │  - SrStatus     │
│  - SysStatus    │         └────────┬────────┘
└─────────────────┘                  │
                                     │ 1
                                     │
                                     │ N
                       ┌─────────────▼────────┐
                       │  SysRoleAuth         │
                       │  - SraRoleId (PK,FK) │
                       │  - SraMenuId (PK,FK) │
                       └─────────────┬────────┘
                                     │ N
                                     │
                                     │ 1
                       ┌─────────────▼────────┐
                       │    SysMenu           │
                       │  - SmId (PK)         │
                       │  - SmParentId        │
                       │  - SmName            │
                       │  - SmType            │
                       │  - SmUrl             │
                       │  - SmLevel           │
                       └──────────────────────┘
```

### 4.3 迁移管理

**创建迁移**:
```bash
dotnet ef migrations add InitialCreate --project WinformTemplate
```

**应用迁移**:
```bash
dotnet ef database update --project WinformTemplate
```

**回滚迁移**:
```bash
dotnet ef database update PreviousMigration --project WinformTemplate
```

**删除最后一次迁移**:
```bash
dotnet ef migrations remove --project WinformTemplate
```

### 4.4 查询优化

**使用 AsNoTracking**:
```csharp
// 只读查询，不需要跟踪实体变化
public async Task<IEnumerable<SysAccountModel>> GetAllAccountsAsync()
{
    return await _dbSet.AsNoTracking().ToListAsync();
}
```

**预加载关联数据**:
```csharp
// Include 预加载
public async Task<SysAccountModel> GetAccountWithRoleAsync(int id)
{
    return await _dbSet
        .Include(a => a.Role)
        .Include(a => a.Extend)
        .FirstOrDefaultAsync(a => a.SysId == id);
}
```

**分页查询**:
```csharp
public async Task<(IEnumerable<SysAccountModel> items, int total)> GetPagedAccountsAsync(
    int page, int pageSize, string searchTerm = null)
{
    var query = _dbSet.AsQueryable();

    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(a =>
            a.SysAccountName.Contains(searchTerm) ||
            a.SysNickname.Contains(searchTerm));
    }

    var total = await query.CountAsync();
    var items = await query
        .OrderBy(a => a.SysId)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (items, total);
}
```

---

## 5. 业务逻辑层

### 5.1 Service 层职责

**核心职责**:
1. **业务规则实现** - 封装复杂的业务逻辑
2. **事务管理** - 协调多个 Repository 操作
3. **数据验证** - 业务层面的数据验证
4. **权限控制** - 业务操作的权限检查
5. **日志记录** - 关键操作的审计日志

### 5.2 Service 设计模式

**示例：账户管理服务**:

```csharp
public interface ISysAccountService
{
    Task<SysAccountModel> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(SysAccountModel account);
    Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword);
    Task<bool> FreezeAccountAsync(int accountId, string reason);
    Task<IEnumerable<SysAccountModel>> GetAccountsByRoleAsync(int roleId);
}

public class SysAccountService : ISysAccountService
{
    private readonly ISysAccountRepository _accountRepository;
    private readonly ISysRoleRepository _roleRepository;
    private readonly ILogger _logger;

    public SysAccountService(
        ISysAccountRepository accountRepository,
        ISysRoleRepository roleRepository,
        ILogger logger)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<SysAccountModel> LoginAsync(string username, string password)
    {
        // 1. 参数验证
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("用户名和密码不能为空");
        }

        // 2. 查询账户
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            _logger.Warn($"登录失败：用户不存在 - {username}");
            return null;
        }

        // 3. 验证密码
        var hashedPassword = MD5Helper.Hash(password);
        if (account.SysPassword != hashedPassword)
        {
            _logger.Warn($"登录失败：密码错误 - {username}");
            return null;
        }

        // 4. 检查账户状态
        if (account.SysStatus != 0)
        {
            _logger.Warn($"登录失败：账户已冻结 - {username}");
            throw new InvalidOperationException("账户已被冻结");
        }

        // 5. 更新最后登录时间
        account.SysUpdateAt = DateTime.Now;
        await _accountRepository.UpdateAsync(account);

        _logger.Info($"用户登录成功 - {username}");
        return account;
    }

    public async Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword)
    {
        // 1. 获取账户
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new ArgumentException("账户不存在");
        }

        // 2. 验证旧密码
        var oldPasswordHash = MD5Helper.Hash(oldPassword);
        if (account.SysPassword != oldPasswordHash)
        {
            _logger.Warn($"修改密码失败：旧密码错误 - AccountId: {accountId}");
            return false;
        }

        // 3. 更新新密码
        account.SysPassword = MD5Helper.Hash(newPassword);
        account.SysUpdateAt = DateTime.Now;
        await _accountRepository.UpdateAsync(account);

        _logger.Info($"密码修改成功 - AccountId: {accountId}");
        return true;
    }
}
```

### 5.3 事务管理

**使用 DbContext 事务**:

```csharp
public async Task<bool> TransferRoleAsync(int fromAccountId, int toAccountId, int roleId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. 移除原账户角色
        var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
        fromAccount.SysRoleId = 0;  // 默认角色
        await _accountRepository.UpdateAsync(fromAccount);

        // 2. 分配新角色
        var toAccount = await _accountRepository.GetByIdAsync(toAccountId);
        toAccount.SysRoleId = roleId;
        await _accountRepository.UpdateAsync(toAccount);

        // 3. 提交事务
        await transaction.CommitAsync();
        _logger.Info($"角色转移成功：{fromAccountId} -> {toAccountId}, RoleId: {roleId}");
        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.Error($"角色转移失败：{ex.Message}", ex);
        return false;
    }
}
```

---

## 6. 用户界面层

### 6.1 主窗体设计

**MainForm 结构**:

```csharp
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        InitializeUI();
    }

    private void InitializeUI()
    {
        // 设置标题
        this.Text = $"WinformTemplate v{GlobalProjectConfig.Instance.AppVersion}";

        // 初始化分割面板
        splitContainer1.SplitterDistance = (int)(this.Height * 0.8);

        // 加载用户控件
        LoadUserControls();

        // 重定向控制台输出
        Console.SetOut(new LabelWriter(lblConsole));
    }

    private void LoadUserControls()
    {
        // 创建用户控件实例
        var testPage1 = new UCPageTestPage1();
        var testPage2 = new UCPageTestPage2();
        var verifyPage = new UCPage_Verify();

        // 添加到 Tab 控件
        AddTabPage("测试页面1", testPage1);
        AddTabPage("测试页面2", testPage2);
        AddTabPage("验证页面", verifyPage);
    }

    private void AddTabPage(string title, UserControl control)
    {
        var tabPage = new TabPage(title);
        control.Dock = DockStyle.Fill;
        tabPage.Controls.Add(control);
        tabControl1.TabPages.Add(tabPage);
    }
}
```

### 6.2 UserControl 设计

**标准 UserControl 结构**:

```csharp
public partial class UCPageTestPage1 : UserControl
{
    private readonly TestPageViewModel _viewModel;

    public UCPageTestPage1()
    {
        InitializeComponent();
        _viewModel = new TestPageViewModel();
        SetupBindings();
        InitializeData();
    }

    private void SetupBindings()
    {
        // 数据绑定
        txtInput.DataBindings.Add(nameof(TextBox.Text), _viewModel, nameof(_viewModel.InputText));
        lblResult.DataBindings.Add(nameof(Label.Text), _viewModel, nameof(_viewModel.ResultText));

        // 命令绑定
        btnProcess.Click += (s, e) => _viewModel.ProcessCommand.Execute(null);
    }

    private async void InitializeData()
    {
        await _viewModel.LoadDataAsync();
    }

    // 释放资源
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _viewModel?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

### 6.3 AntdUI 控件使用

**常用 AntdUI 控件**:

```csharp
// Button 按钮
var btn = new AntdUI.Button
{
    Text = "确定",
    Type = AntdUI.TTypeMini.Primary,
    Size = AntdUI.TSize.Large
};
btn.Click += (s, e) => { /* 处理点击 */ };

// Input 输入框
var input = new AntdUI.Input
{
    PlaceholderText = "请输入...",
    PrefixSvg = "SearchOutlined"
};

// Select 下拉框
var select = new AntdUI.Select
{
    Items = new[] { "选项1", "选项2", "选项3" }
};
select.SelectedIndexChanged += (s, e) => { /* 处理选择 */ };

// Table 表格
var table = new AntdUI.Table
{
    Columns = new[]
    {
        new AntdUI.Column("ID", "Id"),
        new AntdUI.Column("名称", "Name"),
        new AntdUI.Column("状态", "Status")
    },
    DataSource = dataList
};
```

### 6.4 控制台输出重定向

**LabelWriter 实现**:

```csharp
public class LabelWriter : TextWriter
{
    private readonly Label _label;

    public LabelWriter(Label label)
    {
        _label = label;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        UpdateLabel(value.ToString());
    }

    public override void Write(string value)
    {
        UpdateLabel(value);
    }

    public override void WriteLine(string value)
    {
        UpdateLabel(value + Environment.NewLine);
    }

    private void UpdateLabel(string text)
    {
        if (_label.InvokeRequired)
        {
            _label.Invoke(new Action(() =>
            {
                _label.Text += text;
                _label.Refresh();
            }));
        }
        else
        {
            _label.Text += text;
            _label.Refresh();
        }
    }
}

// 使用
Console.SetOut(new LabelWriter(lblConsole));
Console.WriteLine("应用程序已启动");
```

---

## 7. 公共基础设施

### 7.1 日志系统

**位置**: `Src/Logger/Debug.cs`

**配置**: `Resources/Log4net/log4net.config`

**日志级别**:
- **Info** - 一般信息记录
- **Warn** - 警告信息
- **Error** - 错误信息
- **Fatal** - 致命错误

**使用示例**:

```csharp
using WinformTemplate.Logger;

// 信息日志
Debug.Info("应用程序启动成功");
Debug.Info($"当前用户：{username}");

// 警告日志
Debug.Warn("配置文件缺失，使用默认配置");

// 错误日志
try
{
    // 业务操作
}
catch (Exception ex)
{
    Debug.Error("操作失败", ex);
}

// 致命错误
try
{
    // 关键操作
}
catch (Exception ex)
{
    Debug.Fatal("应用程序崩溃", ex);
    Application.Exit();
}
```

**log4net.config 配置**:

```xml
<log4net>
  <!-- Info 级别日志 -->
  <appender name="InfoRollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="Log\LogInfo\" />
    <appendToFile value="true" />
    <rollingStyle value="Date" />
    <datePattern value="yyyyMMdd'.html'" />
    <staticLogFileName value="false" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="&lt;div style='color:blue'&gt;[%date] [%level] %message&lt;/div&gt;" />
    </layout>
    <filter type="log4net.Filter.LevelRangeFilter">
      <levelMin value="INFO" />
      <levelMax value="INFO" />
    </filter>
  </appender>

  <!-- Error 级别日志 -->
  <appender name="ErrorRollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="Log\LogError\" />
    <appendToFile value="true" />
    <rollingStyle value="Date" />
    <datePattern value="yyyyMMdd'.html'" />
    <staticLogFileName value="false" />
    <maximumFileSize value="10MB" />
    <maxSizeRollBackups value="100" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="&lt;div style='color:red'&gt;[%date] [%level] %message %exception&lt;/div&gt;" />
    </layout>
    <filter type="log4net.Filter.LevelRangeFilter">
      <levelMin value="ERROR" />
      <levelMax value="ERROR" />
    </filter>
  </appender>

  <root>
    <level value="ALL" />
    <appender-ref ref="InfoRollingFileAppender" />
    <appender-ref ref="ErrorRollingFileAppender" />
  </root>
</log4net>
```

### 7.2 配置管理

**全局配置加载**:

```csharp
public class GlobalProjectConfig : SingletonBase<GlobalProjectConfig>
{
    public string DbConfig { get; private set; }
    public string AppVersion { get; private set; }
    public string AppName { get; private set; }

    public GlobalProjectConfig()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        var configPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources", "Config", "config.json"
        );

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"配置文件未找到：{configPath}");
        }

        var json = File.ReadAllText(configPath);
        var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        DbConfig = config.ContainsKey("DB") ? config["DB"] : throw new Exception("数据库配置缺失");
        AppVersion = "0.1.0";
        AppName = "WinformTemplate";
    }

    public bool CheckConfigLoaded()
    {
        return !string.IsNullOrEmpty(DbConfig);
    }
}
```

**配置文件接口**:

```csharp
public interface ILoadConfig
{
    void Load(string filePath);
    string Get(string key);
    void Set(string key, string value);
    void Save(string filePath);
}

// JSON 配置实现
public class LoadJsonConfig : ILoadConfig
{
    private Dictionary<string, string> _config;

    public void Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        _config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }

    public string Get(string key)
    {
        return _config.ContainsKey(key) ? _config[key] : null;
    }

    public void Set(string key, string value)
    {
        _config[key] = value;
    }

    public void Save(string filePath)
    {
        var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}

// INI 配置实现
public class LoadINIConfig : ILoadConfig
{
    // INI 配置实现...
}
```

### 7.3 加密工具

**DES 对称加密**:

```csharp
public class DESHelper : ISymmetricEncryption
{
    private const string DefaultKey = "12345678";  // 8 字节

    public string Encrypt(string plainText, string key = DefaultKey)
    {
        using var des = DES.Create();
        des.Key = Encoding.UTF8.GetBytes(key);
        des.IV = Encoding.UTF8.GetBytes(key);

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText, string key = DefaultKey)
    {
        using var des = DES.Create();
        des.Key = Encoding.UTF8.GetBytes(key);
        des.IV = Encoding.UTF8.GetBytes(key);

        var buffer = Convert.FromBase64String(cipherText);
        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
```

**RSA 非对称加密**:

```csharp
public class RSAHelper
{
    public static (string publicKey, string privateKey) GenerateKeys(int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);
        var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
        return (publicKey, privateKey);
    }

    public static string Encrypt(string plainText, string publicKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);

        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherText, string privateKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

        var data = Convert.FromBase64String(cipherText);
        var decrypted = rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);

        return Encoding.UTF8.GetString(decrypted);
    }
}
```

### 7.4 文件操作

**Excel 交互 (NPOI)**:

```csharp
public class ExcelInteractive
{
    public IWorkbook Read(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        if (Path.GetExtension(filePath) == ".xls")
        {
            return new HSSFWorkbook(stream);  // Excel 2003
        }
        else
        {
            return new XSSFWorkbook(stream);  // Excel 2007+
        }
    }

    public void Write(IWorkbook workbook, string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        workbook.Write(stream);
    }

    public ISheet GetSheet(IWorkbook workbook, int sheetIndex)
    {
        return workbook.GetSheetAt(sheetIndex);
    }

    public ISheet GetSheet(IWorkbook workbook, string sheetName)
    {
        return workbook.GetSheet(sheetName);
    }
}
```

**文件选择器**:

```csharp
public class FileSelector
{
    public static string SelectFile(string filter = "所有文件|*.*", string title = "选择文件")
    {
        using var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title,
            Multiselect = false
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }

    public static string[] SelectFiles(string filter = "所有文件|*.*", string title = "选择文件")
    {
        using var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title,
            Multiselect = true
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileNames : Array.Empty<string>();
    }

    public static string SelectFolder(string description = "选择文件夹")
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = description
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
```

---

## 8. 配置与日志

### 8.1 应用启动流程

```csharp
static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 1. 初始化日志系统
        Debug.InitLog4Net();
        Debug.Info("应用程序启动");

        // 2. 加载全局配置
        if (!GlobalProjectConfig.Instance.CheckConfigLoaded())
        {
            MessageBox.Show("配置加载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        Debug.Info("配置加载成功");

        // 3. 配置依赖注入 (可选)
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // 4. 初始化数据库 (可选)
        // var dbService = serviceProvider.GetRequiredService<SysDbContextService>();
        // await dbService.EnsureCreatedAsync();
        // await dbService.InitializeDatabaseAsync();

        // 5. 启动主窗体
        Application.Run(new MainForm());
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // 注册 DbContext
        services.AddDbContext<SysDbContext>(ServiceLifetime.Scoped);

        // 注册 Repositories
        services.AddScoped<ISysAccountRepository, SysAccountRepository>();
        services.AddScoped<ISysMenuRepository, SysMenuRepository>();
        services.AddScoped<ISysRoleRepository, SysRoleRepository>();
        services.AddScoped<ISysParamRepository, SysParamRepository>();

        // 注册 Services
        services.AddScoped<SysDbContextService>();
        services.AddScoped<ISysAccountService, SysAccountService>();

        // 注册 Forms
        services.AddTransient<MainForm>();
    }
}
```

### 8.2 日志最佳实践

**日志级别选择**:

```csharp
// Info - 记录正常的业务流程
Debug.Info("用户登录成功：admin");
Debug.Info($"数据查询完成，共 {count} 条记录");

// Warn - 记录潜在问题，但不影响系统运行
Debug.Warn("配置文件使用默认值");
Debug.Warn($"连接超时，正在重试第 {retryCount} 次");

// Error - 记录错误，但系统仍可继续运行
try
{
    await SaveDataAsync();
}
catch (Exception ex)
{
    Debug.Error("数据保存失败", ex);
    MessageBox.Show("保存失败，请稍后重试");
}

// Fatal - 记录致命错误，系统无法继续运行
try
{
    InitializeCriticalComponent();
}
catch (Exception ex)
{
    Debug.Fatal("关键组件初始化失败，应用程序即将退出", ex);
    Application.Exit();
}
```

**结构化日志**:

```csharp
// 使用结构化信息
Debug.Info($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [User: {username}] [Action: Login] 登录成功");

// 记录关键业务数据
Debug.Info($"[Order] 订单创建成功 - OrderId: {orderId}, UserId: {userId}, Amount: {amount}");
```

---

## 9. 依赖注入

### 9.1 DI 容器配置

**服务注册**:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISysAccountRepository, SysAccountRepository>();
        services.AddScoped<ISysMenuRepository, SysMenuRepository>();
        services.AddScoped<ISysRoleRepository, SysRoleRepository>();
        services.AddScoped<ISysParamRepository, SysParamRepository>();
        return services;
    }

    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<SysDbContextService>();
        services.AddScoped<ISysAccountService, SysAccountService>();
        return services;
    }

    public static IServiceCollection AddDatabaseContext(this IServiceCollection services)
    {
        services.AddDbContext<SysDbContext>(ServiceLifetime.Scoped);
        return services;
    }
}

// 使用
var services = new ServiceCollection();
services.AddDatabaseContext()
        .AddRepositories()
        .AddBusinessServices();
var serviceProvider = services.BuildServiceProvider();
```

### 9.2 服务生命周期

**Transient** - 每次请求都创建新实例:
```csharp
services.AddTransient<MainForm>();  // 每次获取都是新实例
```

**Scoped** - 在同一作用域内共享实例:
```csharp
services.AddScoped<SysDbContext>();  // 同一作用域共享
services.AddScoped<ISysAccountRepository, SysAccountRepository>();
```

**Singleton** - 整个应用程序生命周期单例:
```csharp
services.AddSingleton<IConfiguration>(GlobalProjectConfig.Instance);
```

### 9.3 服务解析

**构造函数注入**:

```csharp
public class SysAccountService : ISysAccountService
{
    private readonly ISysAccountRepository _accountRepository;
    private readonly ISysRoleRepository _roleRepository;

    // 通过构造函数注入依赖
    public SysAccountService(
        ISysAccountRepository accountRepository,
        ISysRoleRepository roleRepository)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
    }
}
```

**手动解析服务**:

```csharp
using (var scope = serviceProvider.CreateScope())
{
    var accountService = scope.ServiceProvider.GetRequiredService<ISysAccountService>();
    var account = await accountService.LoginAsync("admin", "123456");
}
```

---

## 10. 数据流向

### 10.1 查询操作流程

```
┌──────────┐  User Input   ┌──────────┐  Data Binding  ┌──────────────┐
│   View   │──────────────>│ ViewModel│───────────────>│   Service    │
│(UI Layer)│               │  (MVVM)  │                │(Business)    │
└──────────┘               └──────────┘                └──────┬───────┘
     ▲                                                        │
     │                                                        │ Call
     │                                                        ▼
     │                                                ┌───────────────┐
     │                                                │  Repository   │
     │                                                │(Data Access)  │
     │                                                └───────┬───────┘
     │                                                        │
     │                                                        │ EF Core
     │                                                        ▼
     │                                                ┌───────────────┐
     │ Update UI                                      │   DbContext   │
     │<───────────────────────────────────────────────┤   (ORM)       │
                                                     └───────┬───────┘
                                                             │
                                                             │ SQL
                                                             ▼
                                                     ┌───────────────┐
                                                     │   Database    │
                                                     │   (MySQL)     │
                                                     └───────────────┘
```

**代码示例**:

```csharp
// 1. UI - 用户点击查询按钮
private void btnSearch_Click(object sender, EventArgs e)
{
    _viewModel.SearchCommand.Execute(txtSearch.Text);
}

// 2. ViewModel - 处理命令
public ICommand SearchCommand { get; }

public UserListViewModel()
{
    SearchCommand = new RelayCommand<string>(ExecuteSearch);
}

private async void ExecuteSearch(string keyword)
{
    await ExecuteAsync(async () =>
    {
        var users = await _userService.SearchUsersAsync(keyword);
        Users = new ObservableCollection<UserModel>(users);
    });
}

// 3. Service - 业务逻辑
public async Task<IEnumerable<UserModel>> SearchUsersAsync(string keyword)
{
    return await _userRepository.GetByConditionAsync(u =>
        u.Username.Contains(keyword) || u.Nickname.Contains(keyword));
}

// 4. Repository - 数据访问
public async Task<IEnumerable<UserModel>> GetByConditionAsync(
    Expression<Func<UserModel, bool>> predicate)
{
    return await _dbSet.Where(predicate).ToListAsync();
}

// 5. DbContext - EF Core 生成 SQL 并执行

// 6. Database - 返回数据

// 7. ViewModel - 更新集合触发 PropertyChanged

// 8. UI - 数据绑定自动更新界面
```

### 10.2 新增/更新操作流程

```
User Input (View)
    │
    ├──> ViewModel (Validation)
    │       │
    │       ├──> Service (Business Rules)
    │       │       │
    │       │       ├──> Repository (Add/Update)
    │       │       │       │
    │       │       │       ├──> DbContext (Track Changes)
    │       │       │       │       │
    │       │       │       │       └──> Database (Execute SQL)
    │       │       │       │
    │       │       │       └──> SaveChangesAsync()
    │       │       │
    │       │       └──> Transaction Management
    │       │
    │       └──> Update UI State
    │
    └──> Show Success Message
```

---

## 11. 最佳实践

### 11.1 代码规范

**命名约定**:
- 类名：PascalCase（如 `SysAccountModel`）
- 接口：以 I 开头（如 `IRepository`）
- 方法：PascalCase（如 `GetUserByIdAsync`）
- 私有字段：_camelCase（如 `_repository`）
- 异步方法：以 Async 结尾（如 `LoadDataAsync`）

**注释规范**:

```csharp
/// <summary>
/// 根据用户名获取账户信息
/// </summary>
/// <param name="username">用户名</param>
/// <returns>账户模型，如果不存在返回 null</returns>
public async Task<SysAccountModel> GetByUsernameAsync(string username)
{
    // 实现代码
}
```

### 11.2 异常处理

**统一异常处理**:

```csharp
public class GlobalExceptionHandler
{
    public static void HandleException(Exception ex, string context)
    {
        // 记录日志
        Debug.Error($"[{context}] 发生异常", ex);

        // 根据异常类型处理
        switch (ex)
        {
            case ArgumentException:
            case ArgumentNullException:
                MessageBox.Show($"参数错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;

            case InvalidOperationException:
                MessageBox.Show($"操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;

            case DbUpdateException:
                MessageBox.Show("数据库操作失败，请稍后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;

            default:
                MessageBox.Show($"系统错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
        }
    }
}
```

### 11.3 性能优化

**异步编程**:
```csharp
// 使用 async/await
public async Task LoadDataAsync()
{
    IsBusy = true;
    try
    {
        var data = await _repository.GetAllAsync();
        Data = new ObservableCollection<Model>(data);
    }
    finally
    {
        IsBusy = false;
    }
}
```

**数据分页**:
```csharp
public async Task LoadPageAsync(int page, int pageSize)
{
    var (items, total) = await _repository.GetPagedAsync(page, pageSize);
    Items = new ObservableCollection<Model>(items);
    TotalPages = (int)Math.Ceiling((double)total / pageSize);
}
```

**使用 AsNoTracking**:
```csharp
// 只读查询不需要跟踪
public async Task<IEnumerable<Model>> GetReadOnlyDataAsync()
{
    return await _dbSet.AsNoTracking().ToListAsync();
}
```

### 11.4 安全性

**密码加密**:
```csharp
// 使用 MD5 (示例，实际应使用更安全的算法如 BCrypt)
public static string HashPassword(string password)
{
    using var md5 = MD5.Create();
    var bytes = Encoding.UTF8.GetBytes(password);
    var hash = md5.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
```

**SQL 注入防护**:
```csharp
// 使用参数化查询 (EF Core 自动处理)
var users = await _dbSet.Where(u => u.Username == username).ToListAsync();
// ✓ 安全

// 避免字符串拼接
// var sql = $"SELECT * FROM Users WHERE Username = '{username}'";
// ✗ 不安全
```

**输入验证**:
```csharp
public async Task<bool> RegisterAsync(SysAccountModel account)
{
    // 验证必填字段
    if (string.IsNullOrWhiteSpace(account.SysAccountName))
        throw new ArgumentException("用户名不能为空");

    // 验证格式
    if (!Regex.IsMatch(account.SysAccountName, @"^[a-zA-Z0-9_]{4,20}$"))
        throw new ArgumentException("用户名格式不正确");

    // 验证长度
    if (account.SysPassword.Length < 6)
        throw new ArgumentException("密码长度不能少于 6 位");

    // 业务逻辑
    await _repository.AddAsync(account);
}
```

### 11.5 测试策略

**单元测试示例**:

```csharp
[TestFixture]
public class SysAccountRepositoryTests
{
    private SysDbContext _context;
    private SysAccountRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<SysDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new SysDbContext(options);
        _repository = new SysAccountRepository(_context);
    }

    [Test]
    public async Task GetByUsernameAsync_ShouldReturnAccount_WhenExists()
    {
        // Arrange
        var account = new SysAccountModel
        {
            SysAccountName = "testuser",
            SysPassword = "hashedpassword"
        };
        await _repository.AddAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUsernameAsync("testuser");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("testuser", result.SysAccountName);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
}
```

---

## 附录

### A. 常用命令

**EF Core 迁移**:
```bash
# 添加迁移
dotnet ef migrations add <MigrationName>

# 应用迁移
dotnet ef database update

# 回滚迁移
dotnet ef database update <PreviousMigration>

# 删除最后一次迁移
dotnet ef migrations remove

# 生成SQL脚本
dotnet ef migrations script
```

**NuGet 包管理**:
```bash
# 安装包
dotnet add package <PackageName>

# 更新包
dotnet add package <PackageName> --version <Version>

# 移除包
dotnet remove package <PackageName>

# 列出包
dotnet list package
```

### B. 参考资源

- [Microsoft .NET 文档](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core 文档](https://docs.microsoft.com/ef/core/)
- [AntdUI GitHub](https://github.com/AntdUI/AntdUI)
- [NPOI 文档](https://github.com/nissl-lab/npoi)
- [log4net 文档](https://logging.apache.org/log4net/)

---

**文档版本**: 1.0.0
**最后更新**: 2025-12-16
**维护者**: Mustenaka
