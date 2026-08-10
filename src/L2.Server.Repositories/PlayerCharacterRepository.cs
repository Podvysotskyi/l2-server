using L2.Server.Context;
using L2.Server.Context.Entities;
using L2.Server.Context.Identifiers;
using L2.Server.Contracts;
using L2.Server.Exceptions;
using L2.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace L2.Server.Repositories;

public sealed class PlayerCharacterRepository(IDbContextFactory<L2ServerDbContext> contextFactory)
    : IPlayerCharacterRepository
{
    public async Task<int> CleanupExpiredAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Characters
                .Where(character => character.DeleteAfter <= now)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Expired character cleanup failed.", exception);
        }
    }

    public async Task<IReadOnlyList<PlayerCharacterSummary>> ListAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var mageClassIds = await context.PlayerClasses.AsNoTracking()
                .Where(playerClass => playerClass.IsMage)
                .Select(playerClass => playerClass.Id)
                .Distinct()
                .ToListAsync(cancellationToken);
            var mageIds = mageClassIds.ToHashSet();
            var characters = await context.Characters.AsNoTracking()
                .Where(character => character.AccountId == accountId)
                .OrderBy(character => character.AccountSlot)
                .ToListAsync(cancellationToken);
            return characters.Select(character => ToSummary(
                character,
                mageIds.Contains(character.BaseClassId))).ToArray();
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Player character listing failed.", exception);
        }
    }

    public async Task<CharacterCreationOptions> GetCreationOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var roots = await context.PlayerClasses.AsNoTracking()
                .Where(playerClass => playerClass.ParentClassId == null)
                .Include(playerClass => playerClass.PlayerRace)
                .Include(playerClass => playerClass.PlayerSex)
                .OrderBy(playerClass => playerClass.Id)
                .ThenBy(playerClass => playerClass.PlayerRaceId)
                .ThenBy(playerClass => playerClass.PlayerSexId)
                .ToListAsync(cancellationToken);
            var faces = await context.PlayerFaces.AsNoTracking().ToListAsync(cancellationToken);
            var styles = await context.PlayerHairStyles.AsNoTracking().ToListAsync(cancellationToken);
            var colors = await context.PlayerHairColors.AsNoTracking().ToListAsync(cancellationToken);

            return new CharacterCreationOptions(0,
                roots.GroupBy(playerClass => new
                    { playerClass.Id, playerClass.Name, playerClass.IsMage })
                    .Select(classGroup => new RootClassOption(
                        (int)classGroup.Key.Id,
                        classGroup.Key.Name,
                        classGroup.Key.IsMage,
                        classGroup.GroupBy(playerClass => new
                            { playerClass.PlayerRaceId, playerClass.PlayerRace.Name })
                            .Select(raceGroup => new RaceOption(
                                (int)raceGroup.Key.PlayerRaceId,
                                raceGroup.Key.Name,
                                raceGroup.Select(playerClass => new SexOption(
                                        (int)playerClass.PlayerSexId,
                                        playerClass.PlayerSex.Name,
                                        Appearance(faces, playerClass.PlayerRaceId, playerClass.PlayerSexId),
                                        Appearance(styles, playerClass.PlayerRaceId, playerClass.PlayerSexId),
                                        Appearance(colors, playerClass.PlayerRaceId, playerClass.PlayerSexId)))
                                    .OrderBy(sex => sex.Id).ToArray()))
                            .OrderBy(race => race.Id).ToArray()))
                    .OrderBy(rootClass => rootClass.Id).ToArray());
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Character creation option lookup failed.", exception);
        }
    }

    public async Task<CharacterMutationResult> CreateAsync(
        CharacterCreationData character,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            var accountLock = BitConverter.ToInt64(character.AccountId.ToByteArray());
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock({accountLock})", cancellationToken);
            var usedSlots = await context.Characters
                .Where(existing => existing.AccountId == character.AccountId)
                .Select(existing => existing.AccountSlot)
                .ToListAsync(cancellationToken);
            var slot = Enumerable.Range(0, character.MaximumCharacters)
                .FirstOrDefault(candidate => !usedSlots.Contains(candidate), -1);
            if (slot < 0)
            {
                return new(false, "character_limit");
            }

            var entity = new PlayerCharacter
            {
                Id = Guid.NewGuid(),
                AccountId = character.AccountId,
                AccountSlot = slot,
                Name = character.Name,
                NormalizedName = character.NormalizedName,
                PlayerRaceId = (PlayerRaceId)character.RaceId,
                PlayerSexId = (PlayerSexId)character.SexId,
                BaseClassId = (PlayerClassId)character.ClassId,
                ActiveClassId = (PlayerClassId)character.ClassId,
                FaceId = character.FaceId,
                HairStyleId = character.HairStyleId,
                HairColorId = character.HairColorId,
                CreatedAt = character.CreatedAt,
                UpdatedAt = character.CreatedAt
            };
            context.Characters.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            var isMage = await IsMageAsync(context, entity, cancellationToken);
            return new(true, Character: ToSummary(entity, isMage));
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            return new(false, postgres.ConstraintName == "ix_characters_normalized_name"
                ? "name_taken"
                : "character_limit");
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Player character creation failed.", exception);
        }
    }

    public async Task<CharacterMutationResult> SelectAsync(
        Guid accountId,
        Guid characterId,
        CancellationToken cancellationToken = default) =>
        await FindOwnedAsync(accountId, characterId, false, cancellationToken);

    public async Task<CharacterMutationResult> ScheduleDeletionAsync(
        Guid accountId,
        Guid characterId,
        DateTimeOffset deleteAfter,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var character = await context.Characters.SingleOrDefaultAsync(existing =>
                existing.Id == characterId && existing.AccountId == accountId, cancellationToken);
            if (character is null) return new(false, "character_not_found");
            character.DeleteAfter ??= deleteAfter;
            character.UpdatedAt = now;
            await context.SaveChangesAsync(cancellationToken);
            return new(true, Character: ToSummary(character,
                await IsMageAsync(context, character, cancellationToken)));
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Character deletion scheduling failed.", exception);
        }
    }

    public async Task<CharacterMutationResult> RestoreAsync(
        Guid accountId,
        Guid characterId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var character = await context.Characters.SingleOrDefaultAsync(existing =>
                existing.Id == characterId && existing.AccountId == accountId, cancellationToken);
            if (character is null) return new(false, "character_not_found");
            if (character.DeleteAfter <= now) return new(false, "deletion_expired");
            character.DeleteAfter = null;
            character.UpdatedAt = now;
            await context.SaveChangesAsync(cancellationToken);
            return new(true, Character: ToSummary(character,
                await IsMageAsync(context, character, cancellationToken)));
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Character restoration failed.", exception);
        }
    }

    private async Task<CharacterMutationResult> FindOwnedAsync(
        Guid accountId,
        Guid characterId,
        bool allowDeleting,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var character = await context.Characters.AsNoTracking().SingleOrDefaultAsync(existing =>
                existing.Id == characterId && existing.AccountId == accountId, cancellationToken);
            if (character is null) return new(false, "character_not_found");
            if (!allowDeleting && character.DeleteAfter is not null)
                return new(false, "character_pending_deletion");
            return new(true, Character: ToSummary(character,
                await IsMageAsync(context, character, cancellationToken)));
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Player character lookup failed.", exception);
        }
    }

    private static PlayerCharacterSummary ToSummary(PlayerCharacter character, bool isMage) => new(
        character.Id,
        character.AccountSlot,
        character.Name,
        (int)character.PlayerRaceId,
        (int)character.PlayerSexId,
        (int)character.BaseClassId,
        (int)character.ActiveClassId,
        isMage,
        character.FaceId,
        character.HairStyleId,
        character.HairColorId,
        character.Level,
        character.Experience,
        character.DeleteAfter);

    private static Task<bool> IsMageAsync(
        L2ServerDbContext context,
        PlayerCharacter character,
        CancellationToken cancellationToken) => context.PlayerClasses.AsNoTracking().AnyAsync(playerClass =>
            playerClass.Id == character.BaseClassId &&
            playerClass.PlayerRaceId == character.PlayerRaceId &&
            playerClass.PlayerSexId == character.PlayerSexId &&
            playerClass.IsMage,
            cancellationToken);

    private static AppearanceOption[] Appearance(
        IEnumerable<PlayerFace> values,
        PlayerRaceId raceId,
        PlayerSexId sexId) => values
        .Where(item => item.PlayerRaceId == raceId && item.PlayerSexId == sexId)
        .OrderBy(item => item.Id)
        .Select(item => new AppearanceOption(item.Id, item.Name))
        .ToArray();

    private static AppearanceOption[] Appearance(
        IEnumerable<PlayerHairStyle> values,
        PlayerRaceId raceId,
        PlayerSexId sexId) => values
        .Where(item => item.PlayerRaceId == raceId && item.PlayerSexId == sexId)
        .OrderBy(item => item.Id)
        .Select(item => new AppearanceOption(item.Id, item.Name))
        .ToArray();

    private static AppearanceOption[] Appearance(
        IEnumerable<PlayerHairColor> values,
        PlayerRaceId raceId,
        PlayerSexId sexId) => values
        .Where(item => item.PlayerRaceId == raceId && item.PlayerSexId == sexId)
        .OrderBy(item => item.Id)
        .Select(item => new AppearanceOption(item.Id, item.Name))
        .ToArray();
}
