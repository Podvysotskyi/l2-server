using L2.Server.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.Server.Context;

public sealed partial class L2ServerDbContext
{
    private static void ConfigureGameContent(ModelBuilder modelBuilder)
    {
        var playerRace = modelBuilder.Entity<PlayerRace>();
        playerRace.ToTable("player_races");
        playerRace.HasKey(entity => entity.Id);
        playerRace.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        playerRace.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        playerRace.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_player_races_name");

        var playerSex = modelBuilder.Entity<PlayerSex>();
        playerSex.ToTable("player_sexes");
        playerSex.HasKey(entity => entity.Id);
        playerSex.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        playerSex.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        playerSex.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_player_sexes_name");

        var playerClass = modelBuilder.Entity<PlayerClass>();
        playerClass.ToTable("player_classes");
        playerClass.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerClass.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        playerClass.Property(entity => entity.PlayerSexId).HasColumnName("player_sex_id").ValueGeneratedNever();
        playerClass.Property(entity => entity.PlayerRaceId).HasColumnName("player_race_id").ValueGeneratedNever();
        playerClass.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        playerClass.Property(entity => entity.IsMage).HasColumnName("is_mage");
        playerClass.Property(entity => entity.ParentClassId).HasColumnName("parent_class_id").IsRequired(false);
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
        playerFace.ToTable("player_faces");
        playerFace.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerFace.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        playerFace.Property(entity => entity.PlayerSexId).HasColumnName("player_sex_id").ValueGeneratedNever();
        playerFace.Property(entity => entity.PlayerRaceId).HasColumnName("player_race_id").ValueGeneratedNever();
        playerFace.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        playerFace.HasOne(entity => entity.PlayerRace).WithMany(entity => entity.PlayerFaces)
            .HasForeignKey(entity => entity.PlayerRaceId).OnDelete(DeleteBehavior.Restrict);
        playerFace.HasOne(entity => entity.PlayerSex).WithMany(entity => entity.PlayerFaces)
            .HasForeignKey(entity => entity.PlayerSexId).OnDelete(DeleteBehavior.Restrict);

        var playerHairStyle = modelBuilder.Entity<PlayerHairStyle>();
        playerHairStyle.ToTable("player_hair_styles");
        playerHairStyle.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerHairStyle.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        playerHairStyle.Property(entity => entity.PlayerSexId).HasColumnName("player_sex_id").ValueGeneratedNever();
        playerHairStyle.Property(entity => entity.PlayerRaceId).HasColumnName("player_race_id").ValueGeneratedNever();
        playerHairStyle.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        playerHairStyle.HasOne(entity => entity.PlayerRace).WithMany(entity => entity.PlayerHairStyles)
            .HasForeignKey(entity => entity.PlayerRaceId).OnDelete(DeleteBehavior.Restrict);
        playerHairStyle.HasOne(entity => entity.PlayerSex).WithMany(entity => entity.PlayerHairStyles)
            .HasForeignKey(entity => entity.PlayerSexId).OnDelete(DeleteBehavior.Restrict);

        var playerHairColor = modelBuilder.Entity<PlayerHairColor>();
        playerHairColor.ToTable("player_hair_colors");
        playerHairColor.HasKey(entity => new { entity.Id, entity.PlayerSexId, entity.PlayerRaceId });
        playerHairColor.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        playerHairColor.Property(entity => entity.PlayerSexId).HasColumnName("player_sex_id").ValueGeneratedNever();
        playerHairColor.Property(entity => entity.PlayerRaceId).HasColumnName("player_race_id").ValueGeneratedNever();
        playerHairColor.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        playerHairColor.HasOne(entity => entity.PlayerRace).WithMany(entity => entity.PlayerHairColors)
            .HasForeignKey(entity => entity.PlayerRaceId).OnDelete(DeleteBehavior.Restrict);
        playerHairColor.HasOne(entity => entity.PlayerSex).WithMany(entity => entity.PlayerHairColors)
            .HasForeignKey(entity => entity.PlayerSexId).OnDelete(DeleteBehavior.Restrict);

        var assetImportJob = modelBuilder.Entity<AssetImportJob>();
        assetImportJob.ToTable("asset_import_jobs");
        assetImportJob.HasKey(entity => entity.Id);
        assetImportJob.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        assetImportJob.Property(entity => entity.Kind).HasColumnName("kind").HasMaxLength(64);
        assetImportJob.Property(entity => entity.Status).HasColumnName("status").HasMaxLength(32);
        assetImportJob.Property(entity => entity.SourcePath).HasColumnName("source_path").HasMaxLength(1024);
        assetImportJob.Property(entity => entity.SourceHash).HasColumnName("source_hash").HasMaxLength(64);
        assetImportJob.Property(entity => entity.RequestedAt).HasColumnName("requested_at");
        assetImportJob.Property(entity => entity.StartedAt).HasColumnName("started_at");
        assetImportJob.Property(entity => entity.FinishedAt).HasColumnName("finished_at");
        assetImportJob.Property(entity => entity.TotalCount).HasColumnName("total_count");
        assetImportJob.Property(entity => entity.ProcessedCount).HasColumnName("processed_count");
        assetImportJob.Property(entity => entity.SkippedCount).HasColumnName("skipped_count");
        assetImportJob.Property(entity => entity.WarningsJson).HasColumnName("warnings_json").HasColumnType("jsonb");
        assetImportJob.Property(entity => entity.Error).HasColumnName("error").HasMaxLength(4000);
        assetImportJob.HasIndex(entity => new { entity.Kind, entity.Status, entity.RequestedAt })
            .HasDatabaseName("ix_asset_import_jobs_claim");
        assetImportJob.HasIndex(entity => entity.Kind)
            .IsUnique()
            .HasFilter("\"status\" IN ('queued', 'running')")
            .HasDatabaseName("ix_asset_import_jobs_active_kind");

        var assetCatalog = modelBuilder.Entity<AssetCatalog>();
        assetCatalog.ToTable("asset_catalogs");
        assetCatalog.HasKey(entity => entity.Id);
        assetCatalog.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        assetCatalog.Property(entity => entity.Kind).HasColumnName("kind").HasMaxLength(64);
        assetCatalog.Property(entity => entity.SourceFolder).HasColumnName("source_folder").HasMaxLength(256);
        assetCatalog.Property(entity => entity.SourceHash).HasColumnName("source_hash").HasMaxLength(64);
        assetCatalog.Property(entity => entity.SchemaVersion).HasColumnName("schema_version");
        assetCatalog.Property(entity => entity.Protocol).HasColumnName("protocol");
        assetCatalog.Property(entity => entity.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        assetCatalog.Property(entity => entity.IsActive).HasColumnName("is_active");
        assetCatalog.Property(entity => entity.PublishedAt).HasColumnName("published_at");
        assetCatalog.HasIndex(entity => entity.Kind)
            .IsUnique()
            .HasFilter("is_active")
            .HasDatabaseName("ix_asset_catalogs_active_kind");

        var assetCatalogGroup = modelBuilder.Entity<AssetCatalogGroup>();
        assetCatalogGroup.ToTable("asset_catalog_groups");
        assetCatalogGroup.HasKey(entity => entity.Id);
        assetCatalogGroup.Property(entity => entity.Id).HasColumnName("id");
        assetCatalogGroup.Property(entity => entity.CatalogId).HasColumnName("catalog_id");
        assetCatalogGroup.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(256);
        assetCatalogGroup.Property(entity => entity.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        assetCatalogGroup.HasIndex(entity => new { entity.CatalogId, entity.Name })
            .IsUnique().HasDatabaseName("ix_asset_catalog_groups_catalog_name");
        assetCatalogGroup.HasOne(entity => entity.Catalog).WithMany(entity => entity.Groups)
            .HasForeignKey(entity => entity.CatalogId).OnDelete(DeleteBehavior.Cascade);

        var assetCatalogItem = modelBuilder.Entity<AssetCatalogItem>();
        assetCatalogItem.ToTable("asset_catalog_items");
        assetCatalogItem.HasKey(entity => entity.Id);
        assetCatalogItem.Property(entity => entity.Id).HasColumnName("id");
        assetCatalogItem.Property(entity => entity.CatalogId).HasColumnName("catalog_id");
        assetCatalogItem.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(256);
        assetCatalogItem.Property(entity => entity.GroupName).HasColumnName("group_name").HasMaxLength(256);
        assetCatalogItem.Property(entity => entity.Status).HasColumnName("status").HasMaxLength(32);
        assetCatalogItem.Property(entity => entity.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        assetCatalogItem.HasIndex(entity => new { entity.CatalogId, entity.Name })
            .HasDatabaseName("ix_asset_catalog_items_catalog_name");
        assetCatalogItem.HasIndex(entity => new { entity.CatalogId, entity.GroupName, entity.Name })
            .HasDatabaseName("ix_asset_catalog_items_catalog_group_name");
        assetCatalogItem.HasIndex(entity => new { entity.CatalogId, entity.Status })
            .HasDatabaseName("ix_asset_catalog_items_catalog_status");
        assetCatalogItem.HasOne(entity => entity.Catalog).WithMany(entity => entity.Items)
            .HasForeignKey(entity => entity.CatalogId).OnDelete(DeleteBehavior.Cascade);

        var npcType = modelBuilder.Entity<NpcType>();
        npcType.ToTable("npc_types");
        npcType.HasKey(entity => entity.Id);
        npcType.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        npcType.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        npcType.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_npc_types_name");

        var npcRace = modelBuilder.Entity<NpcRace>();
        npcRace.ToTable("npc_races");
        npcRace.HasKey(entity => entity.Id);
        npcRace.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        npcRace.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        npcRace.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_npc_races_name");

        var npcSex = modelBuilder.Entity<NpcSex>();
        npcSex.ToTable("npc_sexes");
        npcSex.HasKey(entity => entity.Id);
        npcSex.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        npcSex.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        npcSex.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_npc_sexes_name");

        var npc = modelBuilder.Entity<Npc>();
        npc.ToTable("npcs", table => table.HasCheckConstraint("ck_npcs_level", "level BETWEEN 1 AND 255"));
        npc.HasKey(entity => entity.Id);
        npc.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        npc.Property(entity => entity.Level).HasColumnName("level");
        npc.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(100).IsRequired(false);
        npc.Property(entity => entity.NpcTypeId).HasColumnName("npc_type_id");
        npc.Property(entity => entity.NpcRaceId).HasColumnName("npc_race_id").IsRequired(false);
        npc.Property(entity => entity.NpcSexId).HasColumnName("npc_sex_id");
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
        skillIcon.Property(entity => entity.SkillId).HasColumnName("skill_id").ValueGeneratedNever();
        skillIcon.Property(entity => entity.Level).HasColumnName("level").ValueGeneratedNever();
        skillIcon.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);

        var skillOperateType = modelBuilder.Entity<SkillOperateType>();
        skillOperateType.ToTable("skill_operate_types");
        skillOperateType.HasKey(entity => entity.Id);
        skillOperateType.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        skillOperateType.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        skillOperateType.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_skill_operate_types_name");

        var skillTargetType = modelBuilder.Entity<SkillTargetType>();
        skillTargetType.ToTable("skill_target_types");
        skillTargetType.HasKey(entity => entity.Id);
        skillTargetType.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        skillTargetType.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64);
        skillTargetType.HasIndex(entity => entity.Name).IsUnique().HasDatabaseName("ix_skill_target_types_name");

        var skill = modelBuilder.Entity<Skill>();
        skill.ToTable("skills", table => table.HasCheckConstraint("ck_skills_levels", "levels BETWEEN 1 AND 255"));
        skill.HasKey(entity => entity.Id);
        skill.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        skill.Property(entity => entity.Levels).HasColumnName("levels");
        skill.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(100);
        skill.Property(entity => entity.SkillOperateTypeId).HasColumnName("skill_operate_type_id").IsRequired(false);
        skill.Property(entity => entity.SkillTargetTypeId).HasColumnName("skill_target_type_id").IsRequired(false);
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
