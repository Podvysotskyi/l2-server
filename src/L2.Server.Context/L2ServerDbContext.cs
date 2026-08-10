using L2.Server.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.Server.Context;

public sealed partial class L2ServerDbContext(DbContextOptions<L2ServerDbContext> options) : DbContext(options)
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
}
