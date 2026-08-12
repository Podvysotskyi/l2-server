namespace L2.Server.Services;

public sealed record GameServerDefinition(
    string Key,
    string DisplayName,
    bool IsDefault,
    string PublicUrl,
    string HealthUrl);
