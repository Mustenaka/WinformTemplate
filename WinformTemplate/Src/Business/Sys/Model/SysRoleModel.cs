using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.Model;

namespace WinformTemplate.Business.Sys.Model;

/// <summary>
/// 系统角色模型
/// </summary>
[Table("sys_role")]
public class SysRoleModel : BaseModel
{
    private long _srId;
    private string _srName;
    private string _srEnName;
    private string _srRemark;
    private bool? _srStatus;
    private DateTime? _srCreateAt;
    private DateTime? _srUpdateAt;
    private string _srReserved1;
    private string _srReserved2;
    private string _sysReserved3;

    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [Column("sr_id")]
    [Comment("主键")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SrId
    {
        get => _srId;
        set => SetProperty(ref _srId, value);
    }

    /// <summary>
    /// 角色名称（可中文）
    /// </summary>
    [Column("sr_name")]
    [Comment("角色名称（可中文）")]
    [StringLength(64)]
    public string SrName
    {
        get => _srName;
        set => SetProperty(ref _srName, value);
    }

    /// <summary>
    /// 角色名称_英文（做链接匹配）
    /// </summary>
    [Column("sr_en_name")]
    [Comment("角色名称_英文（做链接匹配）")]
    [StringLength(64)]
    public string SrEnName
    {
        get => _srEnName;
        set => SetProperty(ref _srEnName, value);
    }

    /// <summary>
    /// 角色描述
    /// </summary>
    [Column("sr_remark")]
    [Comment("角色描述")]
    [StringLength(255)]
    public string SrRemark
    {
        get => _srRemark;
        set => SetProperty(ref _srRemark, value);
    }

    /// <summary>
    /// 状态 | 0 - 有效, 1 - 无效
    /// </summary>
    [Column("sr_status")]
    [Comment("状态 | 0 - 有效, 1 - 无效")]
    public bool? SrStatus
    {
        get => _srStatus;
        set => SetProperty(ref _srStatus, value);
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