using L2.Server.Contracts;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services.Interfaces;

namespace L2.Server.Services;

public sealed class PlayerCharacterService(IPlayerCharacterRepository repository) : IPlayerCharacterService
{
    public Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
        Guid accountId,
        CancellationToken cancellationToken = default) => repository.ListAsync(accountId, cancellationToken);

    public Task<CharacterCreationOptions> GetCreationOptionsAsync(
        CancellationToken cancellationToken = default) => repository.GetCreationOptionsAsync(cancellationToken);

    public Task<CharacterOperationResult> CreateAsync(
        Guid accountId,
        CharacterCreationRequest request,
        CancellationToken cancellationToken = default) => repository.CreateAsync(accountId, request, cancellationToken);

    public Task<CharacterOperationResult> SelectAsync(
        Guid accountId,
        Guid characterId,
        CancellationToken cancellationToken = default) => repository.SelectAsync(accountId, characterId, cancellationToken);

    public Task<CharacterOperationResult> ScheduleDeletionAsync(
        Guid accountId,
        Guid characterId,
        CancellationToken cancellationToken = default) =>
        repository.ScheduleDeletionAsync(accountId, characterId, cancellationToken);

    public Task<CharacterOperationResult> RestoreAsync(
        Guid accountId,
        Guid characterId,
        CancellationToken cancellationToken = default) => repository.RestoreAsync(accountId, characterId, cancellationToken);
}
