namespace L2.PlayerCharacters;

public sealed record PlayerCharacterSummary(
    Guid Id,
    int AccountSlot,
    string Name,
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

public sealed record CharacterCreationRequest(
    string Name,
    int ClassId,
    int RaceId,
    int SexId,
    int FaceId,
    int HairStyleId,
    int HairColorId);

public sealed record CharacterCreationOptions(
    int MaximumCharacters,
    IReadOnlyList<RootClassOption> Classes);
public sealed record RootClassOption(
    int Id,
    string Name,
    bool IsMage,
    IReadOnlyList<RaceOption> AllowedRaces);
public sealed record RaceOption(int Id, string Name, IReadOnlyList<SexOption> AllowedSexes);
public sealed record SexOption(
    int Id,
    string Name,
    IReadOnlyList<AppearanceOption> Faces,
    IReadOnlyList<AppearanceOption> HairStyles,
    IReadOnlyList<AppearanceOption> HairColors);
public sealed record AppearanceOption(int Id, string Name);

public sealed record CharacterOperationResult(
    bool Succeeded,
    string? ErrorCode = null,
    PlayerCharacterSummary? Character = null);
