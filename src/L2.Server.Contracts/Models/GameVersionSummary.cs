namespace L2.Server.Contracts;

public sealed record GameVersionSummary(
    string Key,
    string DisplayName,
    bool IsDefault,
    string GameClientPath,
    string ClientManifestPath);
