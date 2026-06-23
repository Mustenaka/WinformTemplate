using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Business.Template.Model;

/// <summary>
/// 分类数据模型
/// 支持多级分类结构
/// </summary>
[Table("product_category")]
public class CategoryModel : BaseModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 分类名称（必填）
    /// </summary>
    [Column("name")]
    [Required(ErrorMessage = "分类名称不能为空")]
    [StringLength(64, ErrorMessage = "分类名称长度不能超过64个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 父分类ID（null表示顶级分类）
    /// </summary>
    [Column("parent_id")]
    public long? ParentId { get; set; }

    /// <summary>
    /// 层级（0为顶级，1为二级，以此类推）
    /// </summary>
    [Column("level")]
    public int? Level { get; set; }

    /// <summary>
    /// 排序号（数字越小越靠前）
    /// </summary>
    [Column("sort_order")]
    public int? SortOrder { get; set; }

    /// <summary>
    /// 状态：0-正常, 1-停用
    /// </summary>
    [Column("status")]
    public int? Status { get; set; }

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

    // ==================== 导航属性 ====================

    /// <summary>
    /// 该分类下的所有产品
    /// </summary>
    public ICollection<ProductModel>? Products { get; set; }

    // ==================== 辅助属性（不映射到数据库） ====================

    /// <summary>
    /// 状态文本（用于显示）
    /// </summary>
    [NotMapped]
    public string StatusText => Status switch
    {
        0 => "正常",
        1 => "停用",
        _ => "未知"
    };

    /// <summary>
    /// 产品数量（用于显示）
    /// </summary>
    [NotMapped]
    public int ProductCount => Products?.Count ?? 0;

    /// <summary>
    /// 层级文本（用于树形显示缩进）
    /// </summary>
    [NotMapped]
    public string LevelPrefix
    {
        get
        {
            if (!Level.HasValue || Level.Value == 0) return "";
            return new string(' ', Level.Value * 4) + "└─ ";
        }
    }
}
