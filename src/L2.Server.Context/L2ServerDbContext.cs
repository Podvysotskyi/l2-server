using L2.Server.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.Server.Context;

public sealed class L2ServerDbContext(DbContextOptions<L2ServerDbContext> options) : DbContext(options)
{
    public const string SchemaName = "public";

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountCredential> AccountCredentials => Set<AccountCredential>();
    public DbSet<AccountSession> AccountSessions => Set<AccountSession>();
    public DbSet<AccountLoginHistory> AccountLoginHistory => Set<AccountLoginHistory>();
    public DbSet<GameSessionTicket> GameSessionTickets => Set<GameSessionTicket>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<PlayerCharacter> Characters => Set<PlayerCharacter>();
    public DbSet<Npc> Npcs => Set<Npc>();
    public DbSet<NpcType> NpcTypes => Set<NpcType>();
    public DbSet<NpcRace> NpcRaces => Set<NpcRace>();
    public DbSet<NpcSex> NpcSexes => Set<NpcSex>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<SkillIcon> SkillIcons => Set<SkillIcon>();
    public DbSet<SkillOperateType> SkillOperateTypes => Set<SkillOperateType>();
    public DbSet<SkillTargetType> SkillTargetTypes => Set<SkillTargetType>();
    public DbSet<PlayerRace> PlayerRaces => Set<PlayerRace>();
    public DbSet<PlayerSex> PlayerSexes => Set<PlayerSex>();
    public DbSet<PlayerClass> PlayerClasses => Set<PlayerClass>();
    public DbSet<PlayerFace> PlayerFaces => Set<PlayerFace>();
    public DbSet<PlayerHairStyle> PlayerHairStyles => Set<PlayerHairStyle>();
    public DbSet<PlayerHairColor> PlayerHairColors => Set<PlayerHairColor>();
    public DbSet<AssetImportJob> AssetImportJobs => Set<AssetImportJob>();
    public DbSet<AssetCatalog> AssetCatalogs => Set<AssetCatalog>();
    public DbSet<AssetCatalogGroup> AssetCatalogGroups => Set<AssetCatalogGroup>();
    public DbSet<AssetCatalogItem> AssetCatalogItems => Set<AssetCatalogItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        ConfigurePlayerIdentity(modelBuilder);
        ConfigurePlayerCharacters(modelBuilder);
        ConfigureGameContent(modelBuilder);
    }

    private static void ConfigurePlayerIdentity(ModelBuilder modelBuilder)
    {
        var account = modelBuilder.Entity<Account>();
        account.HasIndex(entity => entity.NormalizedUsername).IsUnique().HasDatabaseName("ix_accounts_normalized_username");
        account.HasIndex(entity => entity.NormalizedEmail).IsUnique().HasDatabaseName("ix_accounts_normalized_email");

        var credential = modelBuilder.Entity<AccountCredential>();
        credential.HasOne(entity => entity.Account)
            .WithOne(entity => entity.Credential)
            .HasForeignKey<AccountCredential>(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        var session = modelBuilder.Entity<AccountSession>();
        session.HasIndex(entity => entity.TokenHash).IsUnique().HasDatabaseName("ix_account_sessions_token_hash");
        session.HasIndex(entity => new { entity.AccountId, entity.ExpiresAt })
            .HasFilter("revoked_at IS NULL")
            .HasDatabaseName("ix_account_sessions_account_active");
        session.HasOne(entity => entity.Account)
            .WithMany(entity => entity.Sessions)
            .HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        var history = modelBuilder.Entity<AccountLoginHistory>();
        history.HasIndex(entity => new { entity.AccountId, entity.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_account_login_history_account_time");
        history.HasOne(entity => entity.Account)
            .WithMany(entity => entity.LoginHistory)
            .HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        var ticket = modelBuilder.Entity<GameSessionTicket>();
        ticket.HasIndex(entity => entity.TokenHash).IsUnique().HasDatabaseName("ix_game_session_tickets_token_hash");
        ticket.HasIndex(entity => entity.AccountSessionId)
            .HasDatabaseName("ix_game_session_tickets_account_session_id");
        ticket.HasIndex(entity => entity.ExpiresAt)
            .HasFilter("consumed_at IS NULL")
            .HasDatabaseName("ix_game_session_tickets_pending_expiry");
        ticket.HasOne(entity => entity.AccountSession)
            .WithMany(entity => entity.GameTickets)
            .HasForeignKey(entity => entity.AccountSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        var gameSession = modelBuilder.Entity<GameSession>();
        gameSession.HasOne(entity => entity.AccountSession)
            .WithMany(entity => entity.GameSessions)
            .HasForeignKey(entity => entity.AccountSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        gameSession.HasIndex(entity => entity.AccountSessionId)
            .HasDatabaseName("ix_game_sessions_account_session_id");
        gameSession.HasIndex(entity => entity.AccessTokenHash).IsUnique()
            .HasDatabaseName("ix_game_sessions_access_token_hash");
        gameSession.HasIndex(entity => new { entity.RevokedAt, entity.ExpiresAt })
            .HasDatabaseName("ix_game_sessions_active_expiry");
    }

    private static void ConfigurePlayerCharacters(ModelBuilder modelBuilder)
    {
        var character = modelBuilder.Entity<PlayerCharacter>();
        character.ToTable("characters", table =>
        {
            table.HasCheckConstraint("ck_characters_level", "level BETWEEN 1 AND 255");
            table.HasCheckConstraint("ck_characters_experience", "experience >= 0");
            table.HasCheckConstraint("ck_characters_account_slot", "account_slot >= 0");
        });
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

    private static void ConfigureGameContent(ModelBuilder modelBuilder)
    {
        var playerRace = modelBuilder.Entity<PlayerRace>();
        playerRace.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_player_races_name");

        var playerSex = modelBuilder.Entity<PlayerSex>();
        playerSex.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_player_sexes_name");

        var playerClass = modelBuilder.Entity<PlayerClass>();
        playerClass.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerClass.HasIndex(entity => new { entity.Name, entity.PlayerSexId, entity.PlayerRaceId })
            .IsUnique().HasDatabaseName("ix_player_classes_name_sex_race");
        playerClass.HasIndex(entity => entity.PlayerRaceId).HasDatabaseName("ix_player_classes_player_race_id");
        playerClass.HasIndex(entity => entity.PlayerSexId).HasDatabaseName("ix_player_classes_player_sex_id");
        playerClass.HasIndex(entity => new { entity.ParentClassId, entity.PlayerSexId, entity.PlayerRaceId })
            .HasDatabaseName("ix_player_classes_parent_sex_race");
        playerClass.HasOne(entity => entity.PlayerRace)
            .WithMany(entity => entity.PlayerClasses)
            .HasForeignKey(entity => entity.PlayerRaceId)
            .OnDelete(DeleteBehavior.Restrict);
        playerClass.HasOne(entity => entity.PlayerSex)
            .WithMany(entity => entity.PlayerClasses)
            .HasForeignKey(entity => entity.PlayerSexId)
            .OnDelete(DeleteBehavior.Restrict);
        playerClass.HasOne(entity => entity.ParentClass)
            .WithMany(entity => entity.ChildClasses)
            .HasForeignKey(entity => new { entity.ParentClassId, entity.PlayerSexId, entity.PlayerRaceId })
            .HasPrincipalKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId })
            .OnDelete(DeleteBehavior.Restrict);

        var playerFace = modelBuilder.Entity<PlayerFace>();
        playerFace.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerFace.HasOne(entity => entity.PlayerRace).WithMany(entity => entity.PlayerFaces)
            .HasForeignKey(entity => entity.PlayerRaceId).OnDelete(DeleteBehavior.Restrict);
        playerFace.HasOne(entity => entity.PlayerSex).WithMany(entity => entity.PlayerFaces)
            .HasForeignKey(entity => entity.PlayerSexId).OnDelete(DeleteBehavior.Restrict);

        var playerHairStyle = modelBuilder.Entity<PlayerHairStyle>();
        playerHairStyle.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerHairStyle.HasOne(entity => entity.PlayerRace).WithMany(entity => entity.PlayerHairStyles)
            .HasForeignKey(entity => entity.PlayerRaceId).OnDelete(DeleteBehavior.Restrict);
        playerHairStyle.HasOne(entity => entity.PlayerSex).WithMany(entity => entity.PlayerHairStyles)
            .HasForeignKey(entity => entity.PlayerSexId).OnDelete(DeleteBehavior.Restrict);

        var playerHairColor = modelBuilder.Entity<PlayerHairColor>();
        playerHairColor.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerHairColor.HasOne(entity => entity.PlayerRace).WithMany(entity => entity.PlayerHairColors)
            .HasForeignKey(entity => entity.PlayerRaceId).OnDelete(DeleteBehavior.Restrict);
        playerHairColor.HasOne(entity => entity.PlayerSex).WithMany(entity => entity.PlayerHairColors)
            .HasForeignKey(entity => entity.PlayerSexId).OnDelete(DeleteBehavior.Restrict);

        var assetImportJob = modelBuilder.Entity<AssetImportJob>();
        assetImportJob.HasIndex(entity => new { entity.Kind, entity.Status, entity.RequestedAt })
            .HasDatabaseName("ix_asset_import_jobs_claim");
        assetImportJob.HasIndex(entity => entity.Kind)
            .IsUnique()
            .HasFilter("\"status\" IN ('queued', 'running')")
            .HasDatabaseName("ix_asset_import_jobs_active_kind");

        var assetCatalog = modelBuilder.Entity<AssetCatalog>();
        assetCatalog.HasIndex(entity => entity.Kind)
            .IsUnique()
            .HasFilter("is_active")
            .HasDatabaseName("ix_asset_catalogs_active_kind");

        var assetCatalogGroup = modelBuilder.Entity<AssetCatalogGroup>();
        assetCatalogGroup.HasIndex(entity => new { entity.CatalogId, entity.Name })
            .IsUnique().HasDatabaseName("ix_asset_catalog_groups_catalog_name");
        assetCatalogGroup.HasOne(entity => entity.Catalog).WithMany(entity => entity.Groups)
            .HasForeignKey(entity => entity.CatalogId).OnDelete(DeleteBehavior.Cascade);

        var assetCatalogItem = modelBuilder.Entity<AssetCatalogItem>();
        assetCatalogItem.HasIndex(entity => new { entity.CatalogId, entity.Name })
            .HasDatabaseName("ix_asset_catalog_items_catalog_name");
        assetCatalogItem.HasIndex(entity => new { entity.CatalogId, entity.GroupName, entity.Name })
            .HasDatabaseName("ix_asset_catalog_items_catalog_group_name");
        assetCatalogItem.HasIndex(entity => new { entity.CatalogId, entity.Status })
            .HasDatabaseName("ix_asset_catalog_items_catalog_status");
        assetCatalogItem.HasOne(entity => entity.Catalog).WithMany(entity => entity.Items)
            .HasForeignKey(entity => entity.CatalogId).OnDelete(DeleteBehavior.Cascade);

        var npcType = modelBuilder.Entity<NpcType>();
        npcType.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_npc_types_name");

        var npcRace = modelBuilder.Entity<NpcRace>();
        npcRace.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_npc_races_name");

        var npcSex = modelBuilder.Entity<NpcSex>();
        npcSex.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_npc_sexes_name");

        var npc = modelBuilder.Entity<Npc>();
        npc.ToTable("npcs", table => table.HasCheckConstraint("ck_npcs_level", "level BETWEEN 1 AND 255"));
        npc.HasIndex(entity => entity.NpcTypeId).HasDatabaseName("ix_npcs_npc_type_id");
        npc.HasIndex(entity => entity.NpcRaceId).HasDatabaseName("ix_npcs_npc_race_id");
        npc.HasIndex(entity => entity.NpcSexId).HasDatabaseName("ix_npcs_npc_sex_id");
        npc.HasOne(entity => entity.NpcType)
            .WithMany(entity => entity.Npcs)
            .HasForeignKey(entity => entity.NpcTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        npc.HasOne(entity => entity.NpcRace)
            .WithMany(entity => entity.Npcs)
            .HasForeignKey(entity => entity.NpcRaceId)
            .OnDelete(DeleteBehavior.Restrict);
        npc.HasOne(entity => entity.NpcSex)
            .WithMany(entity => entity.Npcs)
            .HasForeignKey(entity => entity.NpcSexId)
            .OnDelete(DeleteBehavior.Restrict);

        var skillIcon = modelBuilder.Entity<SkillIcon>();
        skillIcon.ToTable(
            "skill_icons",
            table => table.HasCheckConstraint("ck_skill_icons_level", "level BETWEEN 1 AND 255"));
        skillIcon.HasKey(entity => new { entity.SkillId, entity.Level });

        var skillOperateType = modelBuilder.Entity<SkillOperateType>();
        skillOperateType.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_skill_operate_types_name");

        var skillTargetType = modelBuilder.Entity<SkillTargetType>();
        skillTargetType.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_skill_target_types_name");

        var skill = modelBuilder.Entity<Skill>();
        skill.ToTable("skills", table => table.HasCheckConstraint("ck_skills_levels", "levels BETWEEN 1 AND 255"));
        skill.HasIndex(entity => entity.SkillOperateTypeId).HasDatabaseName("ix_skills_skill_operate_type_id");
        skill.HasIndex(entity => entity.SkillTargetTypeId).HasDatabaseName("ix_skills_skill_target_type_id");
        skill.HasOne(entity => entity.SkillOperateType)
            .WithMany(entity => entity.Skills)
            .HasForeignKey(entity => entity.SkillOperateTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        skill.HasOne(entity => entity.SkillTargetType)
            .WithMany(entity => entity.Skills)
            .HasForeignKey(entity => entity.SkillTargetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        skill.HasMany(entity => entity.SkillIcons)
            .WithOne(entity => entity.Skill)
            .HasForeignKey(entity => entity.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
