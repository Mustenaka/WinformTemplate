using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Sys.Model;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Sys.Context;

/// <summary>
/// 系统数据库上下文 sys_db
///     一套DbContext管理所有系统模型
/// </summary>
public class SysDbContext : DbContext
{
    /// <summary>
    /// 系统账户
    /// </summary>
    public DbSet<SysAccountModel> SysAccounts { get; set; }

    /// <summary>
    /// 系统菜单
    /// </summary>
    public DbSet<SysMenuModel> SysMenus { get; set; }

    /// <summary>
    /// 系统参数
    /// </summary>
    public DbSet<SysParamModel> SysParams { get; set; }

    /// <summary>
    /// 系统角色
    /// </summary>
    public DbSet<SysRoleModel> SysRoles { get; set; }

    /// <summary>
    /// 系统角色授权
    /// </summary>
    public DbSet<SysRoleAuthModel> SysRoleAuths { get; set; }

    /// <summary>
    /// 系统账户扩展
    /// </summary>
    public DbSet<SysExtendModel> SysExtends { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public SysDbContext(DbContextOptions<SysDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 配置数据库连接
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var connectionString = GlobalProjectConfig.Instance.Config?.DB;
        optionsBuilder.UseMySql(connectionString,
            new MySqlServerVersion(new Version(8, 0, 21))
        );
    }

    /// <summary>
    /// 模型创建配置
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置复合主键
        modelBuilder.Entity<SysRoleAuthModel>()
            .HasKey(ra => new { ra.SraRoleId, ra.SraMenuId });

        // 配置角色和菜单之间的多对多关系
        modelBuilder.Entity<SysRoleAuthModel>()
            .HasOne(ra => ra.Role)
            .WithMany()
            .HasForeignKey(ra => ra.SraRoleId)
            .OnDelete(DeleteBehavior.Cascade); // 级联删除

        modelBuilder.Entity<SysRoleAuthModel>()
            .HasOne(ra => ra.Menu)
            .WithMany()
            .HasForeignKey(ra => ra.SraMenuId)
            .OnDelete(DeleteBehavior.Cascade); // 级联删除

        // 可以在这里添加其他实体关系配置和索引配置

        // 配置自定义SQL类型映射（如果需要）
        // modelBuilder.Entity<SysAccount>()
        //     .Property(a => a.SysStatus)
        //     .HasColumnType("tinyint(1)");
    }
}