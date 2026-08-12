namespace L2.Server.Contracts;

public sealed record PlayerCharacterSummary(
    Guid Id,
    int AccountSlot,
    string Name,
    string GameVersion,
    int RaceId,
    int SexId,
    int BaseClassId,
    int ActiveClassId,
    bool IsMage,
    int FaceId,
    int HairStyleId,
    int HairColorId,
    short Level,
    long Experience,
    DateTimeOffset? DeleteAfter);
