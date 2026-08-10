namespace L2.GameServer.Sessions;

public sealed record SessionAuthenticate(
    string Type,
    string Ticket,
    int ProtocolVersion,
    string ClientBuild,
    string AssetRelease,
    string GameDataRelease);

public sealed record AuthenticatedAccount(Guid AccountId, string Username);

public sealed record CharacterCreateMessage(
    string Type,
    string Name,
    int ClassId,
    int RaceId,
    int SexId,
    int FaceId,
    int HairStyleId,
    int HairColorId);
