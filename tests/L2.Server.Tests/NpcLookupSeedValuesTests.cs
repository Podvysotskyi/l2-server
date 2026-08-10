using L2.GameContent.Identifiers;
using L2.GameContent.Seeding;
using Xunit;

namespace L2.Foundation.Tests;

public sealed class NpcLookupSeedValuesTests
{
    [Fact]
    public void Seed_values_cover_each_enum_once()
    {
        AssertComplete(NpcLookupSeedValues.Types, Enum.GetValues<NpcTypeId>());
        AssertComplete(NpcLookupSeedValues.Races, Enum.GetValues<NpcRaceId>());
        AssertComplete(NpcLookupSeedValues.Sexes, Enum.GetValues<NpcSexId>());
    }

    [Fact]
    public void Npc_catalog_contains_valid_unique_definitions()
    {
        Assert.Equal(6519, NpcSeedValues.Npcs.Count);
        Assert.Equal(
            NpcSeedValues.Npcs.Count,
            NpcSeedValues.Npcs.Select(value => value.Id).Distinct().Count());
        Assert.All(NpcSeedValues.Npcs, value =>
        {
            Assert.True(value.Id > 0);
            Assert.InRange(value.Level, (short)1, (short)255);
            Assert.True(value.Name is null || value.Name.Length <= 100);
            Assert.True(Enum.IsDefined(value.NpcTypeId));
            Assert.True(value.NpcRaceId is null || Enum.IsDefined(value.NpcRaceId.Value));
            Assert.True(Enum.IsDefined(value.NpcSexId));
        });
        Assert.Equal(50, NpcSeedValues.Npcs.Count(value => value.Name is null));
        Assert.DoesNotContain(NpcSeedValues.Npcs, value => value.Name == string.Empty);

        var gremlin = Assert.Single(NpcSeedValues.Npcs, value => value.Id == 20001);
        Assert.Equal("Gremlin", gremlin.Name);
        Assert.Equal(NpcTypeId.Monster, gremlin.NpcTypeId);
        Assert.Equal(NpcRaceId.Fairy, gremlin.NpcRaceId);
        Assert.Equal(NpcSexId.Male, gremlin.NpcSexId);
    }

    private static void AssertComplete<TId>(
        IReadOnlyList<(TId Id, string Name)> values,
        IReadOnlyList<TId> enumValues)
        where TId : struct, Enum
    {
        Assert.Equal(enumValues.Order(), values.Select(value => value.Id).Order());
        Assert.Equal(values.Count, values.Select(value => value.Id).Distinct().Count());
        Assert.Equal(values.Count, values.Select(value => value.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(values, value => string.IsNullOrWhiteSpace(value.Name));
    }
}
