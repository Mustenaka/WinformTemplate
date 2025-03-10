using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Business.Sys.Model;

/// <summary>
/// 系统菜单类
/// </summary>
[Table("sys_menu")]
public class SysMenuModel : BaseModel
{
    private long _smId;
    private long _smParentId;
    private string _smName;
    private string _smEnName;
    private short _smType;
    private string _smUrl;
    private string _smTarget;
    private long? _smLevel;
    private int? _smSort;
    private string _smIcon;
    private string _smRemark;
    private bool? _sysStatus;
    private DateTime? _sysCreateAt;
    private DateTime? _sysUpdateAt;
    private string _sysReserved1;
    private string _sysReserved2;
    private string _sysReserved3;

    /// <summary>
    /// 菜单主键id
    /// </summary>
    [Key]
    [Column("sm_id")]
    [Comment("菜单主键id")]
    public long SmId
    {
        get => _smId;
        set => SetProperty(ref _smId, value);
    }

    /// <summary>
    /// 父级id
    /// </summary>
    [Column("sm_parent_id")]
    [Comment("父级id")]
    public long SmParentId
    {
        get => _smParentId;
        set => SetProperty(ref _smParentId, value);
    }

    /// <summary>
    /// 名称（中文描述）
    /// </summary>
    [Column("sm_name")]
    [Comment("名称（中文描述）")]
    [StringLength(255)]
    [Required]
    public string SmName
    {
        get => _smName;
        set => SetProperty(ref _smName, value);
    }

    /// <summary>
    /// 名称（英文描述）
    /// </summary>
    [Column("sm_en_name")]
    [Comment("名称（英文描述）")]
    [StringLength(255)]
    [Required]
    public string SmEnName
    {
        get => _smEnName;
        set => SetProperty(ref _smEnName, value);
    }

    /// <summary>
    /// 类型 0 - 菜单 ， 1 - 内容
    /// </summary>
    [Column("sm_type")]
    [Comment("类型 0 - 菜单 ， 1 - 内容")]
    [Required]
    public short SmType
    {
        get => _smType;
        set => SetProperty(ref _smType, value);
    }

    /// <summary>
    /// 资源链接类型
    /// </summary>
    [Column("sm_url")]
    [Comment("资源链接类型")]
    [StringLength(255)]
    [Required]
    public string SmUrl
    {
        get => _smUrl;
        set => SetProperty(ref _smUrl, value);
    }

    /// <summary>
    /// 资源地址
    /// </summary>
    [Column("sm_target")]
    [Comment("资源地址")]
    [StringLength(255)]
    [Required]
    public string SmTarget
    {
        get => _smTarget;
        set => SetProperty(ref _smTarget, value);
    }

    /// <summary>
    /// 等级，0为最高
    /// </summary>
    [Column("sm_level")]
    [Comment("等级，0为最高")]
    public long? SmLevel
    {
        get => _smLevel;
        set => SetProperty(ref _smLevel, value);
    }

    /// <summary>
    /// 排序规则，越小越靠前
    /// </summary>
    [Column("sm_sort")]
    [Comment("排序规则，越小越靠前")]
    public int? SmSort
    {
        get => _smSort;
        set => SetProperty(ref _smSort, value);
    }

    /// <summary>
    /// 匹配的logo资源系统
    /// </summary>
    [Column("sm_icon")]
    [Comment("匹配的logo资源系统")]
    [StringLength(255)]
    public string SmIcon
    {
        get => _smIcon;
        set => SetProperty(ref _smIcon, value);
    }

    /// <summary>
    /// 描述
    /// </summary>
    [Column("sm_remark")]
    [Comment("描述")]
    [StringLength(255)]
    public string SmRemark
    {
        get => _smRemark;
        set => SetProperty(ref _smRemark, value);
    }

    /// <summary>
    /// 激活状态 | 0 - 有效, 1 - 无效
    /// </summary>
    [Column("sys_status")]
    [Comment("激活状态 | 0 - 有效, 1 - 无效")]
    public bool? SysStatus
    {
        get => _sysStatus;
        set => SetProperty(ref _sysStatus, value);
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("sys_create_at")]
    [Comment("创建时间")]
    public DateTime? SysCreateAt
    {
        get => _sysCreateAt;
        set => SetProperty(ref _sysCreateAt, value);
    }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column("sys_update_at")]
    [Comment("更新时间")]
    public DateTime? SysUpdateAt
    {
        get => _sysUpdateAt;
        set => SetProperty(ref _sysUpdateAt, value);
    }

    /// <summary>
    /// 保留字段1
    /// </summary>
    [Column("sys_reserved1")]
    [Comment("保留字段1")]
    [StringLength(255)]
    public string SysReserved1
    {
        get => _sysReserved1;
        set => SetProperty(ref _sysReserved1, value);
    }

    /// <summary>
    /// 保留字段2
    /// </summary>
    [Column("sys_reserved2")]
    [Comment("保留字段2")]
    [StringLength(255)]
    public string SysReserved2
    {
        get => _sysReserved2;
        set => SetProperty(ref _sysReserved2, value);
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