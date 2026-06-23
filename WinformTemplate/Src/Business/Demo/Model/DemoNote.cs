using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WinformTemplate.Common.MVVM;

namespace WinformTemplate.Business.Demo.Model;

[Table("demo_notes")]
public sealed class DemoNote : BaseModel
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("title")]
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    [StringLength(4000)]
    public string Content { get; set; } = string.Empty;

    [Column("pinned")]
    public bool Pinned { get; set; }

    [Column("create_at")]
    public DateTime CreateAt { get; set; }

    [Column("update_at")]
    public DateTime UpdateAt { get; set; }

    [NotMapped]
    public string PinnedText => Pinned ? "Yes" : "No";
}
