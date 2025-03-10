using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Business.Sys.Model;

/// <summary>
/// 系统账户类
/// </summary>
[Table("sys_account")]
public class SysAccountModel : BaseModel
{
    private long _sysId;
    private string? _sysUuid;
    private string? _sysAccount;
    private string? _sysPassword;
    private string? _sysNickname;
    private long? _sysLevel;
    private long? _sysRoleId;
    private long? _sysExtendId;
    private bool? _sysStatus;
    private DateTime? _sysCreateAt;
    private DateTime? _sysUpdateAt;
    private string? _sysReserved1;
    private string? _sysReserved2;
    private string? _sysReserved3;

    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [Column("sys_id")]
    [Comment("主键")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SysId
    {
        get => _sysId;
        set => SetProperty(ref _sysId, value);
    }

    /// <summary>
    /// 对于外部暴露的id，使用uuid
    /// </summary>
    [Column("sys_uuid")]
    [Comment("对于外部暴露的id，使用uuid")]
    [StringLength(128)]
    public string? SysUuid
    {
        get => _sysUuid;
        set => SetProperty(ref _sysUuid, value);
    }

    /// <summary>
    /// 账户名
    /// </summary>
    [Column("sys_account")]
    [Comment("账户名")]
    [StringLength(64)]
    public string? SysAccountName
    {
        get => _sysAccount;
        set => SetProperty(ref _sysAccount, value);
    }

    /// <summary>
    /// 密码
    /// </summary>
    [Column("sys_password")]
    [Comment("密码")]
    [StringLength(64)]
    public string? SysPassword
    {
        get => _sysPassword;
        set => SetProperty(ref _sysPassword, value);
    }

    /// <summary>
    /// 别名（用户名，显示名称）
    /// </summary>
    [Column("sys_nickname")]
    [Comment("别名（用户名，显示名称）")]
    [StringLength(32)]
    public string? SysNickname
    {
        get => _sysNickname;
        set => SetProperty(ref _sysNickname, value);
    }

    /// <summary>
    /// 用户级别（0为最高）
    /// </summary>
    [Column("sys_level")]
    [Comment("用户级别（0为最高）")]
    public long? SysLevel
    {
        get => _sysLevel;
        set => SetProperty(ref _sysLevel, value);
    }

    /// <summary>
    /// 用户规则（0 - 管理员）可扩展
    /// </summary>
    [Column("sys_role_id")]
    [Comment("用户规则（0 - 管理员）可扩展")]
    public long? SysRoleId
    {
        get => _sysRoleId;
        set => SetProperty(ref _sysRoleId, value);
    }

    /// <summary>
    /// 扩展id (和账户关联的更多信息)
    /// </summary>
    [Column("sys_extend_id")]
    [Comment("扩展id (和账户关联的更多信息)")]
    public long? SysExtendId
    {
        get => _sysExtendId;
        set => SetProperty(ref _sysExtendId, value);
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
    public string? SysReserved1
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
    public string? SysReserved2
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
    public string? SysReserved3
    {
        get => _sysReserved3;
        set => SetProperty(ref _sysReserved3, value);
    }
}