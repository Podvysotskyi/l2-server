using System.Text.RegularExpressions;
using L2.Server.Contracts;
using L2.Server.Repositories.Interfaces;
using L2.Server.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace L2.Server.Services;

public sealed partial class PlayerCharacterService(
    IPlayerCharacterRepository repository,
    ICharacterCreationContentProvider creationContentProvider,
    IOptions<PlayerCharacterOptions> options,
    TimeProvider timeProvider) : IPlayerCharacterService
{
    private readonly PlayerCharacterOptions options = options.Value;

    public async Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
        Guid accountId,
        string gameVersion,
        CancellationToken cancellationToken = default)
    {
        await repository.CleanupExpiredAsync(timeProvider.GetUtcNow(), cancellationToken);
        return await repository.ListAsync(accountId, gameVersion, cancellationToken);
    }

    public async Task<CharacterCreationOptions> GetCreationOptionsAsync(
        string gameVersion,
        CancellationToken cancellationToken = default)
    {
        var creationOptions = await creationContentProvider.GetAsync(gameVersion, cancellationToken);
        return creationOptions with { MaximumCharacters = options.MaximumCharactersPerAccount };
    }

    public async Task<CharacterOperationResult> CreateAsync(
        Guid accountId,
        string gameVersion,
        CharacterCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (name.Length < options.MinimumNameLength ||
            name.Length > options.MaximumNameLength ||
            !AlphanumericName().IsMatch(name))
        {
            return new(false, "invalid_name");
        }

        var creationOptions = await creationContentProvider.GetAsync(gameVersion, cancellationToken);
        var rootClass = creationOptions.Classes.SingleOrDefault(candidate => candidate.Id == request.ClassId);
        var race = rootClass?.AllowedRaces.SingleOrDefault(candidate => candidate.Id == request.RaceId);
        var sex = race?.AllowedSexes.SingleOrDefault(candidate => candidate.Id == request.SexId);
        if (rootClass is null || race is null || sex is null)
        {
            return new(false, "invalid_class_variant");
        }
        if (!sex.Faces.Any(option => option.Id == request.FaceId) ||
            !sex.HairStyles.Any(option => option.Id == request.HairStyleId) ||
            !sex.HairColors.Any(option => option.Id == request.HairColorId))
        {
            return new(false, "invalid_appearance");
        }

        var now = timeProvider.GetUtcNow();
        await repository.CleanupExpiredAsync(now, cancellationToken);
        return ToResult(await repository.CreateAsync(new CharacterCreationData(
            accountId,
            gameVersion,
            name,
            name.ToUpperInvariant(),
            request.ClassId,
            request.RaceId,
            request.SexId,
            rootClass.IsMage,
            request.FaceId,
            request.HairStyleId,
            request.HairColorId,
            options.MaximumCharactersPerAccount,
            now), cancellationToken));
    }

    public async Task<CharacterOperationResult> SelectAsync(
        Guid accountId,
        string gameVersion,
        Guid characterId,
        CancellationToken cancellationToken = default) =>
        ToResult(await repository.SelectAsync(accountId, gameVersion, characterId, cancellationToken));

    public async Task<CharacterOperationResult> ScheduleDeletionAsync(
        Guid accountId,
        string gameVersion,
        Guid characterId,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        return ToResult(await repository.ScheduleDeletionAsync(
            accountId,
            gameVersion,
            characterId,
            now.AddDays(options.DeletionDelayDays),
            now,
            cancellationToken));
    }

    public async Task<CharacterOperationResult> RestoreAsync(
        Guid accountId,
        string gameVersion,
        Guid characterId,
        CancellationToken cancellationToken = default) =>
        ToResult(await repository.RestoreAsync(
            accountId,
            gameVersion,
            characterId,
            timeProvider.GetUtcNow(),
            cancellationToken));

    private static CharacterOperationResult ToResult(CharacterMutationResult result) => new(
        result.Succeeded,
        result.ErrorCode,
        result.Character);

    [GeneratedRegex("^[A-Za-z0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex AlphanumericName();
}
