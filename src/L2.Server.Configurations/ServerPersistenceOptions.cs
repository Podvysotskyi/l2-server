namespace L2.Server.Configurations;

public sealed class ServerPersistenceOptions
{
    public const string SectionName = "Persistence";

    public bool RunMigrations { get; init; } = true;
}
