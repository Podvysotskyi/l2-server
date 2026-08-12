using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public interface IPlayerCharacterService
{
    Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
        Guid accountId,
        string gameVersion,
        string gameServer,
        CancellationToken cancellationToken = default);
    Task<CharacterCreationOptions> GetCreationOptionsAsync(
        string gameVersion,
        CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> CreateAsync(
        Guid accountId,
        string gameVersion,
        string gameServer,
        CharacterCreationRequest request,
        CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> SelectAsync(
        Guid accountId,
        string gameVersion,
        string gameServer,
        Guid characterId,
        CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> ScheduleDeletionAsync(
        Guid accountId,
        string gameVersion,
        string gameServer,
        Guid characterId,
        CancellationToken cancellationToken = default);
    Task<CharacterOperationResult> RestoreAsync(
        Guid accountId,
        string gameVersion,
        string gameServer,
        Guid characterId,
        CancellationToken cancellationToken = default);
}
