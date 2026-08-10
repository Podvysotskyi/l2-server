namespace L2.Server.Contracts;

public sealed record CharacterCreationRequest(
    string Name,
    int ClassId,
    int RaceId,
    int SexId,
    int FaceId,
    int HairStyleId,
    int HairColorId);
