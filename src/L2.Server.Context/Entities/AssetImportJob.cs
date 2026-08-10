using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("asset_import_jobs")]
public sealed class AssetImportJob
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    [Column("kind"), MaxLength(64)]
    public required string Kind { get; set; }
    [Column("status"), MaxLength(32)]
    public required string Status { get; set; }
    [Column("source_path"), MaxLength(1024)]
    public required string SourcePath { get; set; }
    [Column("source_hash"), MaxLength(64)]
    public string? SourceHash { get; set; }
    [Column("requested_at")]
    public DateTimeOffset RequestedAt { get; set; }
    [Column("started_at")]
    public DateTimeOffset? StartedAt { get; set; }
    [Column("finished_at")]
    public DateTimeOffset? FinishedAt { get; set; }
    [Column("total_count")]
    public int TotalCount { get; set; }
    [Column("processed_count")]
    public int ProcessedCount { get; set; }
    [Column("skipped_count")]
    public int SkippedCount { get; set; }
    [Column("warnings_json", TypeName = "jsonb")]
    public required string WarningsJson { get; set; }
    [Column("error"), MaxLength(4000)]
    public string? Error { get; set; }
}
