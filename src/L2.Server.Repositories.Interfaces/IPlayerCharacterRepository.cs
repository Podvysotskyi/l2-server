using L2.Server.Contracts;

namespace L2.Server.Repositories.Interfaces;

public interface IPlayerCharacterRepository
{
    Task<int> CleanupExpiredAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
        Guid accountId,
        string gameVersion,
        CancellationToken cancellationToken = default);
    Task<CharacterMutationResult> CreateAsync(
        CharacterCreationData character,
        CancellationToken cancellationToken = default);
    Task<CharacterMutationResult> SelectAsync(
        Guid accountId,
        string gameVersion,
        Guid characterId,
        CancellationToken cancellationToken = default);
    Task<CharacterMutationResult> ScheduleDeletionAsync(
        Guid accountId,
        string gameVersion,
        Guid characterId,
        DateTimeOffset deleteAfter,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
    Task<CharacterMutationResult> RestoreAsync(
        Guid accountId,
        string gameVersion,
        Guid characterId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}
