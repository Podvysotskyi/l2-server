using System.Text.Json;
using L2.GameContent.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2.GameContent;

public static class AssetCatalogPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task PublishAsync<TGroup, TItem, TMetadata>(
        GameContentDbContext context,
        Guid id,
        string kind,
        string sourceFolder,
        string sourceHash,
        int schemaVersion,
        int? protocol,
        IReadOnlyList<TGroup> groups,
        Func<TGroup, string> groupName,
        IReadOnlyList<TItem> items,
        Func<TItem, string> itemName,
        Func<TItem, string?> itemGroup,
        Func<TItem, string> itemStatus,
        TMetadata metadata,
        DateTimeOffset publishedAt,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.AssetCatalogs
            .Where(catalog => catalog.Kind == kind && catalog.IsActive)
            .ExecuteUpdateAsync(setters => setters.SetProperty(catalog => catalog.IsActive, false), cancellationToken);

        var catalog = new AssetCatalog
        {
            Id = id,
            Kind = kind,
            SourceFolder = sourceFolder,
            SourceHash = sourceHash,
            SchemaVersion = schemaVersion,
            Protocol = protocol,
            MetadataJson = JsonSerializer.Serialize(metadata, JsonOptions),
            IsActive = true,
            PublishedAt = publishedAt,
            Groups = groups.Select(group => new AssetCatalogGroup
            {
                Name = groupName(group),
                MetadataJson = JsonSerializer.Serialize(group, JsonOptions)
            }).ToList(),
            Items = items.Select(item => new AssetCatalogItem
            {
                Name = itemName(item),
                GroupName = itemGroup(item),
                Status = itemStatus(item),
                MetadataJson = JsonSerializer.Serialize(item, JsonOptions)
            }).ToList()
        };
        context.AssetCatalogs.Add(catalog);
        await context.SaveChangesAsync(cancellationToken);

        await context.AssetCatalogs
            .Where(existing => existing.Kind == kind && !existing.IsActive && existing.Id != id)
            .ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
