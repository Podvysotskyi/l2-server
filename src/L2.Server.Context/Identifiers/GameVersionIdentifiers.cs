namespace L2.Server.Context.Identifiers;

public static class GameVersionIdentifiers
{
    public const string C1 = "c1";
    public const string C4 = "c4";
    public const string Interlude = "interlude";

    public static bool IsKnown(string key) =>
        string.Equals(key, C1, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, C4, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, Interlude, StringComparison.OrdinalIgnoreCase);
}
