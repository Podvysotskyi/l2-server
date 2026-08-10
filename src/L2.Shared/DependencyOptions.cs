namespace L2.Shared;

public sealed class DependencyOptions
{
    public const string SectionName = "Dependencies";

    public bool PostgreSqlRequired { get; init; } = true;
}
