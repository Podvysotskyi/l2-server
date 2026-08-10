using L2.Server.Contracts;

namespace L2.Server.Repositories.Interfaces;

public interface IPlayerCharacterRepository
{
    Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
    Task<CharacterCreationOptions> GetCreationOptionsAsync(CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> CreateAsync(
        Guid accountId,
        CharacterCreationRequest request,
        CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> SelectAsync(
        Guid accountId,
        Guid characterId,
        CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> ScheduleDeletionAsync(
        Guid accountId,
        Guid characterId,
        CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> RestoreAsync(
        Guid accountId,
        Guid characterId,
        CancellationToken cancellationToken = default);
}
