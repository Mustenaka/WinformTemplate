using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Business.Template.Model;

/// <summary>
/// 导入记录模型
/// 记录每次数据导入的详细信息
/// </summary>
[Table("import_record")]
public class ImportRecordModel : BaseModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 导入的文件名
    /// </summary>
    [Column("file_name")]
    [StringLength(256)]
    public string? FileName { get; set; }

    /// <summary>
    /// 总行数
    /// </summary>
    [Column("total_count")]
    public int? TotalCount { get; set; }

    /// <summary>
    /// 成功导入的行数
    /// </summary>
    [Column("success_count")]
    public int? SuccessCount { get; set; }

    /// <summary>
    /// 失败的行数
    /// </summary>
    [Column("fail_count")]
    public int? FailCount { get; set; }

    /// <summary>
    /// 错误信息（JSON格式，包含所有错误详情）
    /// </summary>
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 导入类型：Excel / CSV
    /// </summary>
    [Column("import_type")]
    [StringLength(20)]
    public string? ImportType { get; set; }

    /// <summary>
    /// 导入状态：0-处理中, 1-成功, 2-部分成功, 3-失败
    /// </summary>
    [Column("status")]
    public int? Status { get; set; }

    /// <summary>
    /// 创建时间（导入时间）
    /// </summary>
    [Column("create_at")]
    public DateTime? CreateAt { get; set; }

    /// <summary>
    /// 操作人ID（关联到用户表）
    /// </summary>
    [Column("create_by")]
    public long? CreateBy { get; set; }

    // ==================== 辅助属性（不映射到数据库） ====================

    /// <summary>
    /// 状态文本（用于显示）
    /// </summary>
    [NotMapped]
    public string StatusText => Status switch
    {
        0 => "处理中",
        1 => "成功",
        2 => "部分成功",
        3 => "失败",
        _ => "未知"
    };

    /// <summary>
    /// 成功率（用于显示）
    /// </summary>
    [NotMapped]
    public double SuccessRate
    {
        get
        {
            if (!TotalCount.HasValue || TotalCount.Value == 0) return 0;
            return (SuccessCount ?? 0) * 100.0 / TotalCount.Value;
        }
    }

    /// <summary>
    /// 成功率文本（用于显示）
    /// </summary>
    [NotMapped]
    public string SuccessRateText => $"{SuccessRate:F1}%";

    /// <summary>
    /// 导入结果摘要（用于显示）
    /// </summary>
    [NotMapped]
    public string Summary
    {
        get
        {
            var total = TotalCount ?? 0;
            var success = SuccessCount ?? 0;
            var fail = FailCount ?? 0;

            if (total == 0) return "无数据";
            if (fail == 0) return $"全部成功（{success}条）";
            if (success == 0) return $"全部失败（{fail}条）";
            return $"成功{success}条，失败{fail}条";
        }
    }

    /// <summary>
    /// 导入时间文本（用于显示）
    /// </summary>
    [NotMapped]
    public string CreateAtText => CreateAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
}
