using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace L2.PlayerIdentity;

public static class PlayerIdentityDatabase
{
    public static bool IsUniqueViolation(Exception exception) =>
        exception is DbUpdateException { InnerException: PostgresException postgres } &&
        postgres.SqlState == PostgresErrorCodes.UniqueViolation;

    public static bool IsPersistenceFailure(Exception exception) =>
        exception is DbUpdateException or NpgsqlException;

    public static PlayerIdentityPersistenceException Wrap(string message, Exception exception) =>
        new(message, exception);
}
