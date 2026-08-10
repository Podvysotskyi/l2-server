using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace L2.Server.Repositories;

internal static class PostgreSqlExceptionClassifier
{
    public static bool IsUniqueViolation(Exception exception) =>
        exception is DbUpdateException { InnerException: PostgresException postgres } &&
        postgres.SqlState == PostgresErrorCodes.UniqueViolation;

    public static bool IsPersistenceFailure(Exception exception) =>
        exception is DbUpdateException or NpgsqlException;
}
