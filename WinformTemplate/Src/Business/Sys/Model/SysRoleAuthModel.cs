using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.Model;

namespace WinformTemplate.Business.Sys.Model;

/// <summary>
/// 系统角色授权关联模型
/// </summary>
[Table("sys_role_auth")]
public class SysRoleAuthModel : BaseModel
{
    private long _sraRoleId;
    private long _sraMenuId;

    /// <summary>
    /// 角色id
    /// </summary>
    [Key]
    [Column("sra_role_id", Order = 0)]
    [Comment("角色id")]
    public long SraRoleId
    {
        get => _sraRoleId;
        set => SetProperty(ref _sraRoleId, value);
    }

    /// <summary>
    /// 资源id
    /// </summary>
    [Key]
    [Column("sra_menu_id", Order = 1)]
    [Comment("资源id")]
    public long SraMenuId
    {
        get => _sraMenuId;
        set => SetProperty(ref _sraMenuId, value);
    }

    #region 导航属性

    /// <summary>
    /// 角色导航属性
    /// </summary>
    [ForeignKey("SraRoleId")]
    public virtual SysRoleModel Role { get; set; }

    /// <summary>
    /// 菜单导航属性
    /// </summary>
    [ForeignKey("SraMenuId")]
    public virtual SysMenuModel Menu { get; set; }

    #endregion
}