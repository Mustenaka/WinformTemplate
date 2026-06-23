using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Template.Model;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Template.Context;

/// <summary>
/// 模板数据库上下文
/// 管理产品、分类、导入记录等模板相关的数据模型
/// </summary>
public class TemplateDbContext : DbContext
{
    /// <summary>
    /// 产品数据集
    /// </summary>
    public DbSet<ProductModel> Products { get; set; }

    /// <summary>
    /// 分类数据集
    /// </summary>
    public DbSet<CategoryModel> Categories { get; set; }

    /// <summary>
    /// 导入记录数据集
    /// </summary>
    public DbSet<ImportRecordModel> ImportRecords { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 配置数据库连接
    /// </summary>
    /// <param name="optionsBuilder">选项构建器</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        EfDbContextOptions.UseConfiguredDatabase(optionsBuilder, GlobalProjectConfig.Instance.Config?.Ef);

        // 开启详细错误信息（开发环境）
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    /// <summary>
    /// 模型创建配置
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==================== Product 配置 ====================

        // 配置产品表
        modelBuilder.Entity<ProductModel>(entity =>
        {
            // 配置索引
            entity.HasIndex(p => p.Code).IsUnique(); // 产品编码唯一索引
            entity.HasIndex(p => p.Name); // 产品名称索引（用于搜索）
            entity.HasIndex(p => p.CategoryId); // 分类ID索引（用于关联查询）
            entity.HasIndex(p => p.Status); // 状态索引（用于筛选）
            entity.HasIndex(p => p.CreateAt); // 创建时间索引（用于排序）

            // 配置外键关系
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // 删除分类时，产品的CategoryId设为NULL
        });

        // ==================== Category 配置 ====================

        // 配置分类表
        modelBuilder.Entity<CategoryModel>(entity =>
        {
            // 配置索引
            entity.HasIndex(c => c.Name); // 分类名称索引
            entity.HasIndex(c => c.ParentId); // 父分类索引
            entity.HasIndex(c => c.SortOrder); // 排序号索引
        });

        // ==================== ImportRecord 配置 ====================

        // 配置导入记录表
        modelBuilder.Entity<ImportRecordModel>(entity =>
        {
            // 配置索引
            entity.HasIndex(i => i.CreateAt); // 创建时间索引
            entity.HasIndex(i => i.Status); // 状态索引
            entity.HasIndex(i => i.CreateBy); // 创建人索引
        });
    }

    /// <summary>
    /// 保存更改前的钩子
    /// 自动设置创建时间和更新时间
    /// </summary>
    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// 保存更改前的钩子（异步）
    /// 自动设置创建时间和更新时间
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 设置时间戳
    /// </summary>
    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries();
        var now = DateTime.Now;

        foreach (var entry in entries)
        {
            // 处理 ProductModel
            if (entry.Entity is ProductModel product)
            {
                if (entry.State == EntityState.Added)
                {
                    product.CreateAt ??= now;
                    product.UpdateAt = now;
                    product.Uuid ??= Guid.NewGuid().ToString("N"); // 生成UUID
                }
                else if (entry.State == EntityState.Modified)
                {
                    product.UpdateAt = now;
                }
            }

            // 处理 CategoryModel
            if (entry.Entity is CategoryModel category)
            {
                if (entry.State == EntityState.Added)
                {
                    category.CreateAt ??= now;
                    category.UpdateAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    category.UpdateAt = now;
                }
            }

            // 处理 ImportRecordModel
            if (entry.Entity is ImportRecordModel importRecord)
            {
                if (entry.State == EntityState.Added)
                {
                    importRecord.CreateAt ??= now;
                }
            }
        }
    }
}
