using L2.Server.Context.Identifiers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("player_faces")]
public sealed class PlayerFace
{
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    [Column("player_sex_id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public PlayerSexId PlayerSexId { get; set; }
    [Column("player_race_id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public PlayerRaceId PlayerRaceId { get; set; }
    [Column("name"), MaxLength(64)]
    public required string Name { get; set; }
    public PlayerSex PlayerSex { get; set; } = null!;
    public PlayerRace PlayerRace { get; set; } = null!;
}
