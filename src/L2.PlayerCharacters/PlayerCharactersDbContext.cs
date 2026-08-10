using L2.PlayerCharacters.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.PlayerCharacters;

public sealed class PlayerCharactersDbContext(DbContextOptions<PlayerCharactersDbContext> options)
    : DbContext(options)
{
    public const string SchemaName = "player";
    public DbSet<PlayerCharacter> Characters => Set<PlayerCharacter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        var character = modelBuilder.Entity<PlayerCharacter>();
        character.ToTable("characters", table =>
        {
            table.HasCheckConstraint("ck_characters_level", "level BETWEEN 1 AND 255");
            table.HasCheckConstraint("ck_characters_experience", "experience >= 0");
            table.HasCheckConstraint("ck_characters_account_slot", "account_slot >= 0");
        });
        character.HasKey(entity => entity.Id);
        character.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        character.Property(entity => entity.AccountId).HasColumnName("account_id");
        character.Property(entity => entity.AccountSlot).HasColumnName("account_slot");
        character.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(16);
        character.Property(entity => entity.NormalizedName).HasColumnName("normalized_name").HasMaxLength(16);
        character.Property(entity => entity.PlayerRaceId).HasColumnName("player_race_id");
        character.Property(entity => entity.PlayerSexId).HasColumnName("player_sex_id");
        character.Property(entity => entity.BaseClassId).HasColumnName("base_class_id");
        character.Property(entity => entity.ActiveClassId).HasColumnName("active_class_id");
        character.Property(entity => entity.FaceId).HasColumnName("face_id");
        character.Property(entity => entity.HairStyleId).HasColumnName("hair_style_id");
        character.Property(entity => entity.HairColorId).HasColumnName("hair_color_id");
        character.Property(entity => entity.Level).HasColumnName("level");
        character.Property(entity => entity.Experience).HasColumnName("experience");
        character.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        character.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        character.Property(entity => entity.DeleteAfter).HasColumnName("delete_after");
        character.HasIndex(entity => entity.NormalizedName).IsUnique()
            .HasDatabaseName("ix_characters_normalized_name");
        character.HasIndex(entity => new { entity.AccountId, entity.AccountSlot }).IsUnique()
            .HasDatabaseName("ix_characters_account_slot");
        character.HasIndex(entity => new { entity.AccountId, entity.CreatedAt })
            .HasDatabaseName("ix_characters_account_created");
        character.HasIndex(entity => entity.DeleteAfter)
            .HasFilter("delete_after IS NOT NULL")
            .HasDatabaseName("ix_characters_deletion_deadline");
    }
}
