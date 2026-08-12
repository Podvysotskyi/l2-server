using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("characters")]
public sealed class PlayerCharacter
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    [Column("account_id")]
    public Guid AccountId { get; set; }
    [Column("game_version"), MaxLength(32)]
    public required string GameVersion { get; set; }
    [Column("game_server"), MaxLength(64)]
    public required string GameServer { get; set; }
    [Column("account_slot")]
    public int AccountSlot { get; set; }
    [Column("name"), MaxLength(16)]
    public required string Name { get; set; }
    [Column("normalized_name"), MaxLength(16)]
    public required string NormalizedName { get; set; }
    [Column("player_race_id")]
    public PlayerRaceId PlayerRaceId { get; set; }
    [Column("player_sex_id")]
    public PlayerSexId PlayerSexId { get; set; }
    [Column("base_class_id")]
    public PlayerClassId BaseClassId { get; set; }
    [Column("active_class_id")]
    public PlayerClassId ActiveClassId { get; set; }
    [Column("is_mage")]
    public bool IsMage { get; set; }
    [Column("face_id")]
    public int FaceId { get; set; }
    [Column("hair_style_id")]
    public int HairStyleId { get; set; }
    [Column("hair_color_id")]
    public int HairColorId { get; set; }
    [Column("level")]
    public short Level { get; set; } = 1;
    [Column("experience")]
    public long Experience { get; set; }
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    [Column("delete_after")]
    public DateTimeOffset? DeleteAfter { get; set; }
}
