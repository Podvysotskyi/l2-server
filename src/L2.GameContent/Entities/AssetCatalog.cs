namespace L2.GameContent.Entities;

public sealed class AssetCatalog
{
    public Guid Id { get; set; }
    public required string Kind { get; set; }
    public required string SourceFolder { get; set; }
    public required string SourceHash { get; set; }
    public int SchemaVersion { get; set; }
    public int? Protocol { get; set; }
    public required string MetadataJson { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public ICollection<AssetCatalogGroup> Groups { get; set; } = [];
    public ICollection<AssetCatalogItem> Items { get; set; } = [];
}

public sealed class AssetCatalogGroup
{
    public long Id { get; set; }
    public Guid CatalogId { get; set; }
    public required string Name { get; set; }
    public required string MetadataJson { get; set; }
    public AssetCatalog Catalog { get; set; } = null!;
}

public sealed class AssetCatalogItem
{
    public long Id { get; set; }
    public Guid CatalogId { get; set; }
    public required string Name { get; set; }
    public string? GroupName { get; set; }
    public required string Status { get; set; }
    public required string MetadataJson { get; set; }
    public AssetCatalog Catalog { get; set; } = null!;
}
