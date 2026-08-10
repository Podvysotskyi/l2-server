namespace L2.Server.Contracts;

public sealed record GameSessionAuthentication(
    string Type,
    string AccessToken,
    int ProtocolVersion,
    string ClientBuild,
    string AssetRelease,
    string GameDataRelease);
