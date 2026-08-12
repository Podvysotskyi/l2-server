using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("game_versions")]
public sealed class GameVersion
{
    [Key, Column("key"), MaxLength(32)]
    public required string Key { get; set; }
    [Column("display_name"), MaxLength(64)]
    public required string DisplayName { get; set; }
    [Column("sort_order")]
    public int SortOrder { get; set; }
}
