using Microsoft.EntityFrameworkCore;

namespace L2.Server.Repositories.Tests;

public sealed class PostgreSqlExceptionClassifierTests
{
    [Fact]
    public void Persistence_failure_recognizes_entity_framework_update_errors()
    {
        Assert.True(PostgreSqlExceptionClassifier.IsPersistenceFailure(new DbUpdateException()));
    }

    [Fact]
    public void Persistence_failure_rejects_unrelated_errors()
    {
        Assert.False(PostgreSqlExceptionClassifier.IsPersistenceFailure(new InvalidOperationException()));
    }
}
