using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Business.Sys.Model;

/// <summary>
/// 账户信息扩展模型
/// </summary>
[Table("sys_extend")]
public class SysExtendModel : BaseModel
{
    private long _seId;
    private string? _seAvatarUrl;

    /// <summary>
    /// 账户信息扩展id
    /// </summary>
    [Key]
    [Column("se_id")]
    [Comment("账户信息扩展id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SeId
    {
        get => _seId;
        set => SetProperty(ref _seId, value);
    }

    /// <summary>
    /// 头像地址（URL）
    /// </summary>
    [Column("se_avatar_url")]
    [StringLength(255)]
    [Comment("头像地址（URL）")]
    public string? SeAvatarUrl
    {
        get => _seAvatarUrl;
        set => SetProperty(ref _seAvatarUrl, value);
    }
}