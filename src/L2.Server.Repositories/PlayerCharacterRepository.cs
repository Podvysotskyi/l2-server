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
            var characters = await context.Characters.AsNoTracking()
                .Where(character => character.AccountId == accountId)
                .OrderBy(character => character.AccountSlot)
                .ToListAsync(cancellationToken);
            return characters.Select(ToSummary).ToArray();
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Player character listing failed.", exception);
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
                IsMage = character.IsMage,
                FaceId = character.FaceId,
                HairStyleId = character.HairStyleId,
                HairColorId = character.HairColorId,
                CreatedAt = character.CreatedAt,
                UpdatedAt = character.CreatedAt
            };
            context.Characters.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(true, Character: ToSummary(entity));
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
            return new(true, Character: ToSummary(character));
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
            return new(true, Character: ToSummary(character));
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
            return new(true, Character: ToSummary(character));
        }
        catch (Exception exception) when (PostgreSqlExceptionClassifier.IsPersistenceFailure(exception))
        {
            throw new ServerRepositoryException("Player character lookup failed.", exception);
        }
    }

    private static PlayerCharacterSummary ToSummary(PlayerCharacter character) => new(
        character.Id,
        character.AccountSlot,
        character.Name,
        (int)character.PlayerRaceId,
        (int)character.PlayerSexId,
        (int)character.BaseClassId,
        (int)character.ActiveClassId,
        character.IsMage,
        character.FaceId,
        character.HairStyleId,
        character.HairColorId,
        character.Level,
        character.Experience,
        character.DeleteAfter);
}
