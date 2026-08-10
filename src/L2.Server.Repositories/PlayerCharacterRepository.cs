using System.Text.RegularExpressions;
using L2.Server.Context.Identifiers;
using L2.Server.Context;
using L2.Server.Context.Entities;
using L2.Server.Contracts;
using L2.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace L2.Server.Repositories;

public sealed partial class PlayerCharacterRepository(
    IDbContextFactory<L2ServerDbContext> characterContextFactory,
    IDbContextFactory<L2ServerDbContext> contentContextFactory,
    IOptions<PlayerCharacterOptions> options,
    TimeProvider timeProvider) : IPlayerCharacterRepository
{
    public async Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        await CleanupExpiredAsync(cancellationToken);
        await using var characters = await characterContextFactory.CreateDbContextAsync(cancellationToken);
        await using var content = await contentContextFactory.CreateDbContextAsync(cancellationToken);
        var mageClasses = await content.PlayerClasses.AsNoTracking()
            .Where(item => item.IsMage)
            .Select(item => item.Id)
            .Distinct()
            .ToListAsync(cancellationToken);
        var mageIds = mageClasses.ToHashSet();
        var owned = await characters.Characters.AsNoTracking()
            .Where(item => item.AccountId == accountId)
            .OrderBy(item => item.AccountSlot)
            .ToListAsync(cancellationToken);
        return owned.Select(item => ToSummary(item, mageIds.Contains(item.BaseClassId))).ToArray();
    }

    public async Task<CharacterCreationOptions> GetCreationOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var content = await contentContextFactory.CreateDbContextAsync(cancellationToken);
        var roots = await content.PlayerClasses.AsNoTracking()
            .Where(item => item.ParentClassId == null)
            .Include(item => item.PlayerRace)
            .Include(item => item.PlayerSex)
            .OrderBy(item => item.Id).ThenBy(item => item.PlayerRaceId).ThenBy(item => item.PlayerSexId)
            .ToListAsync(cancellationToken);
        var faces = await content.PlayerFaces.AsNoTracking().ToListAsync(cancellationToken);
        var styles = await content.PlayerHairStyles.AsNoTracking().ToListAsync(cancellationToken);
        var colors = await content.PlayerHairColors.AsNoTracking().ToListAsync(cancellationToken);

        return new CharacterCreationOptions(options.Value.MaximumCharactersPerAccount,
            roots.GroupBy(item => new { item.Id, item.Name, item.IsMage })
            .Select(classGroup => new RootClassOption(
                (int)classGroup.Key.Id,
                classGroup.Key.Name,
                classGroup.Key.IsMage,
                classGroup.GroupBy(item => new { item.PlayerRaceId, item.PlayerRace.Name })
                    .Select(raceGroup => new RaceOption(
                        (int)raceGroup.Key.PlayerRaceId,
                        raceGroup.Key.Name,
                        raceGroup.Select(item => new SexOption(
                            (int)item.PlayerSexId,
                            item.PlayerSex.Name,
                            Appearance(faces, item.PlayerRaceId, item.PlayerSexId),
                            Appearance(styles, item.PlayerRaceId, item.PlayerSexId),
                            Appearance(colors, item.PlayerRaceId, item.PlayerSexId)))
                            .OrderBy(item => item.Id).ToArray()))
                    .OrderBy(item => item.Id).ToArray()))
            .OrderBy(item => item.Id).ToArray());
    }

    public async Task<CharacterOperationResult> CreateAsync(
        Guid accountId,
        CharacterCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (name.Length < options.Value.MinimumNameLength ||
            name.Length > options.Value.MaximumNameLength ||
            !AlphanumericName().IsMatch(name))
        {
            return new(false, "invalid_name");
        }

        var classId = (PlayerClassId)request.ClassId;
        var raceId = (PlayerRaceId)request.RaceId;
        var sexId = (PlayerSexId)request.SexId;
        await using var content = await contentContextFactory.CreateDbContextAsync(cancellationToken);
        var rootClass = await content.PlayerClasses.AsNoTracking().SingleOrDefaultAsync(item =>
            item.Id == classId && item.PlayerRaceId == raceId && item.PlayerSexId == sexId &&
            item.ParentClassId == null, cancellationToken);
        if (rootClass is null)
        {
            return new(false, "invalid_class_variant");
        }

        if (!await content.PlayerFaces.AnyAsync(item => item.Id == request.FaceId &&
                item.PlayerRaceId == raceId && item.PlayerSexId == sexId, cancellationToken) ||
            !await content.PlayerHairStyles.AnyAsync(item => item.Id == request.HairStyleId &&
                item.PlayerRaceId == raceId && item.PlayerSexId == sexId, cancellationToken) ||
            !await content.PlayerHairColors.AnyAsync(item => item.Id == request.HairColorId &&
                item.PlayerRaceId == raceId && item.PlayerSexId == sexId, cancellationToken))
        {
            return new(false, "invalid_appearance");
        }

        await CleanupExpiredAsync(cancellationToken);
        await using var characters = await characterContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await characters.Database.BeginTransactionAsync(cancellationToken);
        var accountLock = BitConverter.ToInt64(accountId.ToByteArray());
        await characters.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({accountLock})", cancellationToken);
        var usedSlots = await characters.Characters.Where(item => item.AccountId == accountId)
            .Select(item => item.AccountSlot).ToListAsync(cancellationToken);
        var slot = Enumerable.Range(0, options.Value.MaximumCharactersPerAccount)
            .FirstOrDefault(candidate => !usedSlots.Contains(candidate), -1);
        if (slot < 0)
        {
            return new(false, "character_limit");
        }

        var now = timeProvider.GetUtcNow();
        var character = new PlayerCharacter
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            AccountSlot = slot,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            PlayerRaceId = raceId,
            PlayerSexId = sexId,
            BaseClassId = classId,
            ActiveClassId = classId,
            FaceId = request.FaceId,
            HairStyleId = request.HairStyleId,
            HairColorId = request.HairColorId,
            CreatedAt = now,
            UpdatedAt = now
        };
        characters.Characters.Add(character);
        try
        {
            await characters.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            return new(false, postgres.ConstraintName == "ix_characters_normalized_name"
                ? "name_taken"
                : "character_limit");
        }

        return new(true, Character: ToSummary(character, rootClass.IsMage));
    }

    public Task<CharacterOperationResult> SelectAsync(Guid accountId, Guid characterId,
        CancellationToken cancellationToken = default) => FindOwnedAsync(accountId, characterId, false, cancellationToken);

    public async Task<CharacterOperationResult> ScheduleDeletionAsync(Guid accountId, Guid characterId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await characterContextFactory.CreateDbContextAsync(cancellationToken);
        var character = await context.Characters.SingleOrDefaultAsync(item =>
            item.Id == characterId && item.AccountId == accountId, cancellationToken);
        if (character is null) return new(false, "character_not_found");
        character.DeleteAfter ??= timeProvider.GetUtcNow().AddDays(options.Value.DeletionDelayDays);
        character.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
        return new(true, Character: ToSummary(character,
            await IsMageAsync(character, cancellationToken)));
    }

    public async Task<CharacterOperationResult> RestoreAsync(Guid accountId, Guid characterId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await characterContextFactory.CreateDbContextAsync(cancellationToken);
        var character = await context.Characters.SingleOrDefaultAsync(item =>
            item.Id == characterId && item.AccountId == accountId, cancellationToken);
        if (character is null) return new(false, "character_not_found");
        if (character.DeleteAfter <= timeProvider.GetUtcNow()) return new(false, "deletion_expired");
        character.DeleteAfter = null;
        character.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
        return new(true, Character: ToSummary(character,
            await IsMageAsync(character, cancellationToken)));
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await characterContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Characters.Where(item => item.DeleteAfter <= timeProvider.GetUtcNow())
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<CharacterOperationResult> FindOwnedAsync(Guid accountId, Guid characterId,
        bool allowDeleting, CancellationToken cancellationToken)
    {
        await using var context = await characterContextFactory.CreateDbContextAsync(cancellationToken);
        var character = await context.Characters.AsNoTracking().SingleOrDefaultAsync(item =>
            item.Id == characterId && item.AccountId == accountId, cancellationToken);
        if (character is null) return new(false, "character_not_found");
        if (!allowDeleting && character.DeleteAfter is not null) return new(false, "character_pending_deletion");
        return new(true, Character: ToSummary(character,
            await IsMageAsync(character, cancellationToken)));
    }

    private static PlayerCharacterSummary ToSummary(PlayerCharacter item, bool isMage) => new(
        item.Id, item.AccountSlot, item.Name, (int)item.PlayerRaceId, (int)item.PlayerSexId,
        (int)item.BaseClassId, (int)item.ActiveClassId, isMage, item.FaceId,
        item.HairStyleId, item.HairColorId, item.Level, item.Experience, item.DeleteAfter);

    private async Task<bool> IsMageAsync(PlayerCharacter character, CancellationToken cancellationToken)
    {
        await using var content = await contentContextFactory.CreateDbContextAsync(cancellationToken);
        return await content.PlayerClasses.AsNoTracking().AnyAsync(item =>
            item.Id == character.BaseClassId && item.PlayerRaceId == character.PlayerRaceId &&
            item.PlayerSexId == character.PlayerSexId && item.IsMage, cancellationToken);
    }

    private static AppearanceOption[] Appearance(IEnumerable<PlayerFace> values,
        PlayerRaceId raceId, PlayerSexId sexId) => values.Where(item =>
            item.PlayerRaceId == raceId && item.PlayerSexId == sexId)
        .OrderBy(item => item.Id).Select(item => new AppearanceOption(item.Id, item.Name)).ToArray();
    private static AppearanceOption[] Appearance(IEnumerable<PlayerHairStyle> values,
        PlayerRaceId raceId, PlayerSexId sexId) => values.Where(item =>
            item.PlayerRaceId == raceId && item.PlayerSexId == sexId)
        .OrderBy(item => item.Id).Select(item => new AppearanceOption(item.Id, item.Name)).ToArray();
    private static AppearanceOption[] Appearance(IEnumerable<PlayerHairColor> values,
        PlayerRaceId raceId, PlayerSexId sexId) => values.Where(item =>
            item.PlayerRaceId == raceId && item.PlayerSexId == sexId)
        .OrderBy(item => item.Id).Select(item => new AppearanceOption(item.Id, item.Name)).ToArray();

    [GeneratedRegex("^[A-Za-z0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex AlphanumericName();
}
