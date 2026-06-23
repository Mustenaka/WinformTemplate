using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Business.Template.Model;

/// <summary>
/// 产品数据模型
/// 通用的产品实体，可适配任何数据管理场景
/// </summary>
[Table("product_data")]
public class ProductModel : BaseModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// UUID - 外部唯一标识
    /// </summary>
    [Column("uuid")]
    [StringLength(64)]
    public string? Uuid { get; set; }

    /// <summary>
    /// 产品名称（必填）
    /// </summary>
    [Column("name")]
    [Required(ErrorMessage = "产品名称不能为空")]
    [StringLength(128, ErrorMessage = "产品名称长度不能超过128个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 产品编码（唯一）
    /// </summary>
    [Column("code")]
    [StringLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 分类ID（外键）
    /// </summary>
    [Column("category_id")]
    public long? CategoryId { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    [Column("price")]
    [Range(0, double.MaxValue, ErrorMessage = "价格不能为负数")]
    public decimal? Price { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [Column("stock")]
    [Range(0, int.MaxValue, ErrorMessage = "库存不能为负数")]
    public int? Stock { get; set; }

    /// <summary>
    /// 状态：0-正常, 1-停用, 2-缺货
    /// </summary>
    [Column("status")]
    public int? Status { get; set; }

    /// <summary>
    /// 产品描述
    /// </summary>
    [Column("description")]
    [StringLength(512)]
    public string? Description { get; set; }

    /// <summary>
    /// 图片URL
    /// </summary>
    [Column("image_url")]
    [StringLength(256)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 标签（多个标签用逗号分隔）
    /// </summary>
    [Column("tags")]
    [StringLength(256)]
    public string? Tags { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("create_at")]
    public DateTime? CreateAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }

    /// <summary>
    /// 保留字段1（用于未来扩展）
    /// </summary>
    [Column("reserved1")]
    [StringLength(255)]
    public string? Reserved1 { get; set; }

    /// <summary>
    /// 保留字段2（用于未来扩展）
    /// </summary>
    [Column("reserved2")]
    [StringLength(255)]
    public string? Reserved2 { get; set; }

    /// <summary>
    /// 保留字段3（用于未来扩展）
    /// </summary>
    [Column("reserved3")]
    [StringLength(255)]
    public string? Reserved3 { get; set; }

    // ==================== 导航属性 ====================

    /// <summary>
    /// 关联的分类
    /// </summary>
    [ForeignKey("CategoryId")]
    public CategoryModel? Category { get; set; }

    // ==================== 辅助属性（不映射到数据库） ====================

    /// <summary>
    /// 分类名称（用于显示）
    /// </summary>
    [NotMapped]
    public string CategoryName => Category?.Name ?? "未分类";

    /// <summary>
    /// 状态文本（用于显示）
    /// </summary>
    [NotMapped]
    public string StatusText => Status switch
    {
        0 => "正常",
        1 => "停用",
        2 => "缺货",
        _ => "未知"
    };

    /// <summary>
    /// 格式化的价格（用于显示）
    /// </summary>
    [NotMapped]
    public string PriceText => Price.HasValue ? $"¥{Price.Value:F2}" : "未设置";

    /// <summary>
    /// 库存状态（用于显示）
    /// </summary>
    [NotMapped]
    public string StockStatus
    {
        get
        {
            if (!Stock.HasValue) return "未设置";
            if (Stock.Value == 0) return "缺货";
            if (Stock.Value < 10) return "库存不足";
            return "充足";
        }
    }
}
