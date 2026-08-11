namespace L2.Server.Repositories.Interfaces;

public sealed record CharacterCreationData(
    Guid AccountId,
    string Name,
    string NormalizedName,
    int ClassId,
    int RaceId,
    int SexId,
    bool IsMage,
    int FaceId,
    int HairStyleId,
    int HairColorId,
    int MaximumCharacters,
    DateTimeOffset CreatedAt);
