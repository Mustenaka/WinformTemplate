using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WinformTemplate.Business.Sys.Context;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Sys.Service.Full;

/// <summary>
/// 系统数据库上下文服务
/// 提供数据库上下文的管理、注册和基本数据库操作
/// </summary>
public class SysDbContextService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    public SysDbContextService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// 向服务容器注册数据库上下文和相关服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="enableDetailedErrors">是否启用详细错误（开发环境建议开启）</param>
    /// <param name="enableSensitiveDataLogging">是否启用敏感数据日志（仅开发环境开启）</param>
    /// <returns>配置后的服务集合</returns>
    public static IServiceCollection AddSysDatabase(
        IServiceCollection services,
        bool enableDetailedErrors = false,
        bool enableSensitiveDataLogging = false)
    {
        // 获取全局配置中的连接字符串
        var connectionString = GlobalProjectConfig.Instance.Config?.DB;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured. Please check GlobalProjectConfig.");
        }

        // 注册SysDbContext
        services.AddDbContext<SysDbContext>(options =>
        {
            // 使用MySQL数据库
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 21)),
                mysqlOptions =>
                {
                    // 启用重试逻辑，以处理暂时性失败
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    // 设置命令超时
                    mysqlOptions.CommandTimeout(60);
                });

            // 启用详细的错误信息（开发环境）
            if (enableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            // 启用敏感数据记录（仅开发环境）
            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // 注册SysDbContextService
        services.AddSingleton<SysDbContextService>();

        return services;
    }

    /// <summary>
    /// 确保数据库已创建并应用了所有迁移
    /// </summary>
    /// <returns>异步任务</returns>
    public async Task EnsureDatabaseCreatedAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SysDbContext>();

            // 确保数据库已创建
            await context.Database.EnsureCreatedAsync();

            // 如果使用迁移，可以取消下面的注释
            // await context.Database.MigrateAsync();
        }
    }

    /// <summary>
    /// 初始化数据库（添加种子数据）
    /// </summary>
    /// <returns>异步任务</returns>
    public async Task InitializeDatabaseAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SysDbContext>();

            // 检查是否有必要初始化数据
            if (!await context.SysRoles.AnyAsync())
            {
                // 添加默认角色
                var adminRole = new SysRoleModel
                {
                    SrName = "管理员",
                    SrEnName = "Admin",
                    SrStatus = false,
                    SrCreateAt = DateTime.Now
                };

                context.SysRoles.Add(adminRole);
                await context.SaveChangesAsync();

                // 添加默认账户 (假设你的SysAccountModel类已正确定义)
                var adminAccount = new SysAccountModel
                {
                    SysAccountName = "admin",
                    SysPassword = "e10adc3949ba59abbe56e057f20f883e", // 123456的MD5
                    SysNickname = "系统管理员",
                    SysLevel = 0,
                    SysRoleId = adminRole.SrId,
                    SysStatus = false,
                    SysCreateAt = DateTime.Now
                };

                context.SysAccounts.Add(adminAccount);
                await context.SaveChangesAsync();

                // 添加基本菜单
                var systemMenu = new SysMenuModel
                {
                    SmId = 1,
                    SmParentId = 0,
                    SmName = "系统管理",
                    SmEnName = "SystemManagement",
                    SmType = 0, // 菜单类型
                    SmUrl = "#",
                    SmTarget = "_self",
                    SmLevel = 0,
                    SmSort = 1,
                    SmIcon = "setting",
                    SysStatus = false,
                    SysCreateAt = DateTime.Now
                };

                var userMenu = new SysMenuModel
                {
                    SmId = 2,
                    SmParentId = 1,
                    SmName = "用户管理",
                    SmEnName = "UserManagement",
                    SmType = 0,
                    SmUrl = "/sys/user",
                    SmTarget = "_self",
                    SmLevel = 1,
                    SmSort = 1,
                    SmIcon = "user",
                    SysStatus = false,
                    SysCreateAt = DateTime.Now
                };

                var roleMenu = new SysMenuModel
                {
                    SmId = 3,
                    SmParentId = 1,
                    SmName = "角色管理",
                    SmEnName = "RoleManagement",
                    SmType = 0,
                    SmUrl = "/sys/role",
                    SmTarget = "_self",
                    SmLevel = 1,
                    SmSort = 2,
                    SmIcon = "team",
                    SysStatus = false,
                    SysCreateAt = DateTime.Now
                };

                context.SysMenus.AddRange(systemMenu, userMenu, roleMenu);
                await context.SaveChangesAsync();

                // 给管理员角色分配所有菜单权限
                var roleAuths = new[]
                {
                        new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = systemMenu.SmId },
                        new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = userMenu.SmId },
                        new SysRoleAuthModel { SraRoleId = adminRole.SrId, SraMenuId = roleMenu.SmId }
                    };

                context.SysRoleAuths.AddRange(roleAuths);
                await context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// 执行数据库事务操作
    /// </summary>
    /// <param name="action">事务操作</param>
    /// <returns>异步任务</returns>
    public async Task ExecuteInTransactionAsync(Func<SysDbContext, Task> action)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SysDbContext>();

            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    await action(context);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// 执行数据库事务操作，并返回结果
    /// </summary>
    /// <typeparam name="T">返回结果类型</typeparam>
    /// <param name="func">事务操作</param>
    /// <returns>操作结果</returns>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<SysDbContext, Task<T>> func)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SysDbContext>();

            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await func(context);
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
    }

    /// <summary>
    /// 获取一个新的数据库上下文实例
    /// </summary>
    /// <returns>数据库上下文</returns>
    public SysDbContext CreateContext()
    {
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SysDbContext>();
    }
}