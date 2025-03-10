using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.Model;

namespace WinformTemplate.Business.Sys.Model;

/// <summary>
/// 系统参数模型
/// </summary>
[Table("sys_param")]
public class SysParamModel : BaseModel
{
    private long _spId;
    private string _spParamKey;
    private string _spParamValue;
    private bool? _spType;
    private int? _spSort;
    private bool? _spStatus;
    private DateTime? _srCreateAt;
    private DateTime? _srUpdateAt;
    private string _srReserved1;
    private string _srReserved2;
    private string _sysReserved3;

    /// <summary>
    /// 系统参数id
    /// </summary>
    [Key]
    [Column("sp_id")]
    [Comment("系统参数id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SpId
    {
        get => _spId;
        set => SetProperty(ref _spId, value);
    }

    /// <summary>
    /// 参数key
    /// </summary>
    [Column("sp_param_key")]
    [Comment("参数key")]
    [StringLength(255)]
    public string SpParamKey
    {
        get => _spParamKey;
        set => SetProperty(ref _spParamKey, value);
    }

    /// <summary>
    /// 参数value
    /// </summary>
    [Column("sp_param_value")]
    [Comment("参数value")]
    [StringLength(255)]
    public string SpParamValue
    {
        get => _spParamValue;
        set => SetProperty(ref _spParamValue, value);
    }

    /// <summary>
    /// 参数类型
    /// </summary>
    [Column("sp_type")]
    [Comment("参数类型")]
    public bool? SpType
    {
        get => _spType;
        set => SetProperty(ref _spType, value);
    }

    /// <summary>
    /// 排序
    /// </summary>
    [Column("sp_sort")]
    [Comment("排序")]
    public int? SpSort
    {
        get => _spSort;
        set => SetProperty(ref _spSort, value);
    }

    /// <summary>
    /// 状态 | 0 - 有效, 1 - 无效
    /// </summary>
    [Column("sp_status")]
    [Comment("状态 | 0 - 有效, 1 - 无效")]
    public bool? SpStatus
    {
        get => _spStatus;
        set => SetProperty(ref _spStatus, value);
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("sr_create_at")]
    [Comment("创建时间")]
    public DateTime? SrCreateAt
    {
        get => _srCreateAt;
        set => SetProperty(ref _srCreateAt, value);
    }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column("sr_update_at")]
    [Comment("更新时间")]
    public DateTime? SrUpdateAt
    {
        get => _srUpdateAt;
        set => SetProperty(ref _srUpdateAt, value);
    }

    /// <summary>
    /// 保留字段1
    /// </summary>
    [Column("sr_reserved1")]
    [Comment("保留字段1")]
    [StringLength(64)]
    public string SrReserved1
    {
        get => _srReserved1;
        set => SetProperty(ref _srReserved1, value);
    }

    /// <summary>
    /// 保留字段2
    /// </summary>
    [Column("sr_reserved2")]
    [Comment("保留字段2")]
    [StringLength(64)]
    public string SrReserved2
    {
        get => _srReserved2;
        set => SetProperty(ref _srReserved2, value);
    }

    /// <summary>
    /// 保留字段3
    /// </summary>
    [Column("sys_reserved3")]
    [Comment("保留字段3")]
    [StringLength(255)]
    public string SysReserved3
    {
        get => _sysReserved3;
        set => SetProperty(ref _sysReserved3, value);
    }
}