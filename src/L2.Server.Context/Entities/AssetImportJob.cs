namespace L2.Server.Context.Entities;

public sealed class AssetImportJob
{
    public Guid Id { get; set; }
    public required string Kind { get; set; }
    public required string Status { get; set; }
    public required string SourcePath { get; set; }
    public string? SourceHash { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int TotalCount { get; set; }
    public int ProcessedCount { get; set; }
    public int SkippedCount { get; set; }
    public required string WarningsJson { get; set; }
    public string? Error { get; set; }
}
