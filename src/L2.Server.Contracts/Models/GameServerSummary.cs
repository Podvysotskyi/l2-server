namespace L2.Server.Contracts;

public sealed record GameServerSummary(
    string Key,
    string DisplayName,
    bool IsDefault,
    string PublicUrl,
    string Status);
