using L2.GameContent.Identifiers;
using L2.GameContent.Seeding;
using Xunit;

namespace L2.Foundation.Tests;

public sealed class PlayerClassSeedValuesTests
{
    [Fact]
    public void Player_class_catalog_matches_the_interlude_class_hierarchy()
    {
        Assert.Equal(89, PlayerClassSeedValues.PlayerClasses.Count);
        Assert.Equal(
            Enum.GetValues<PlayerClassId>().Order(),
            PlayerClassSeedValues.PlayerClasses.Select(value => value.Id).Order());
        Assert.Equal(
            PlayerClassSeedValues.PlayerClasses.Count,
            PlayerClassSeedValues.PlayerClasses.Select(value => value.Id).Distinct().Count());
        Assert.Equal(
            PlayerClassSeedValues.PlayerClasses.Count,
            PlayerClassSeedValues.PlayerClasses.Select(value => value.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(9, PlayerClassSeedValues.PlayerClasses.Count(value => value.ParentClassId is null));

        var variants = PlayerClassSeedValues.PlayerClasses
            .SelectMany(playerClass => playerClass.AllowedRaces.SelectMany(race =>
                race.AllowedSexIds.Select(sexId => (playerClass, race.Id, SexId: sexId))))
            .ToArray();
        Assert.Equal(178, variants.Length);
        Assert.Equal(
            variants.Length,
            variants.Select(value => (value.playerClass.Id, value.SexId, value.Id)).Distinct().Count());
        Assert.All(variants, value =>
        {
            Assert.True(Enum.IsDefined(value.Id));
            Assert.True(Enum.IsDefined(value.SexId));
        });

        var ids = PlayerClassSeedValues.PlayerClasses.Select(value => value.Id).ToHashSet();
        Assert.All(PlayerClassSeedValues.PlayerClasses, value =>
        {
            Assert.False(string.IsNullOrWhiteSpace(value.Name));
            Assert.True(value.Name.Length <= 64);
            if (value.ParentClassId is { } parentClassId)
            {
                Assert.Contains(parentClassId, ids);
                Assert.NotEqual(value.Id, parentClassId);
            }
        });

        var duelist = Assert.Single(
            PlayerClassSeedValues.PlayerClasses,
            value => value.Id == PlayerClassId.Duelist);
        Assert.Equal("Duelist", duelist.Name);
        Assert.Equal(PlayerClassId.Gladiator, duelist.ParentClassId);

        var evasTemplar = Assert.Single(
            PlayerClassSeedValues.PlayerClasses,
            value => value.Id == PlayerClassId.EvasTemplar);
        Assert.Equal("Eva's Templar", evasTemplar.Name);
        Assert.Equal(PlayerClassId.TempleKnight, evasTemplar.ParentClassId);

        var variantKeys = variants
            .Select(value => (value.playerClass.Id, value.SexId, RaceId: value.Id))
            .ToHashSet();
        Assert.All(
            variants.Where(value => value.playerClass.ParentClassId is not null),
            value => Assert.Contains(
                (value.playerClass.ParentClassId!.Value, value.SexId, value.Id),
                variantKeys));
    }

    [Fact]
    public void Player_lookup_catalogs_cover_each_enum_once()
    {
        Assert.Equal(Enum.GetValues<PlayerRaceId>(), PlayerLookupSeedValues.Races.Select(value => value.Id));
        Assert.Equal(Enum.GetValues<PlayerSexId>(), PlayerLookupSeedValues.Sexes.Select(value => value.Id));
        Assert.Equal(
            PlayerLookupSeedValues.Races.Count,
            PlayerLookupSeedValues.Races.Select(value => value.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            PlayerLookupSeedValues.Sexes.Count,
            PlayerLookupSeedValues.Sexes.Select(value => value.Name).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Mystic_branches_are_mages_and_fighter_branches_are_not()
    {
        var mageRoots = new[]
        {
            PlayerClassId.HumanMystic,
            PlayerClassId.ElvenMystic,
            PlayerClassId.DarkMystic,
            PlayerClassId.OrcMystic
        };
        Assert.All(mageRoots, id => Assert.True(
            Assert.Single(PlayerClassSeedValues.PlayerClasses, value => value.Id == id).IsMage));
        Assert.False(Assert.Single(PlayerClassSeedValues.PlayerClasses,
            value => value.Id == PlayerClassId.HumanFighter).IsMage);
        Assert.False(Assert.Single(PlayerClassSeedValues.PlayerClasses,
            value => value.Id == PlayerClassId.DwarfFighter).IsMage);

        var definitions = PlayerClassSeedValues.PlayerClasses.ToDictionary(value => value.Id);
        Assert.All(definitions.Values.Where(value => value.ParentClassId is not null), value =>
            Assert.Equal(definitions[value.ParentClassId!.Value].IsMage, value.IsMage));
    }

    [Fact]
    public void Appearance_catalogs_have_the_original_logical_ranges_for_every_variant()
    {
        Assert.Equal(30, PlayerAppearanceSeedValues.Faces.Count);
        Assert.Equal(60, PlayerAppearanceSeedValues.HairStyles.Count);
        Assert.Equal(40, PlayerAppearanceSeedValues.HairColors.Count);
        foreach (var raceId in Enum.GetValues<PlayerRaceId>())
        {
            foreach (var sexId in Enum.GetValues<PlayerSexId>())
            {
                Assert.Equal([0, 1, 2], PlayerAppearanceSeedValues.Faces
                    .Where(value => value.PlayerRaceId == raceId && value.PlayerSexId == sexId)
                    .Select(value => value.Id));
                Assert.Equal(Enumerable.Range(0, sexId == PlayerSexId.Male ? 5 : 7),
                    PlayerAppearanceSeedValues.HairStyles
                        .Where(value => value.PlayerRaceId == raceId && value.PlayerSexId == sexId)
                        .Select(value => value.Id));
                Assert.Equal([0, 1, 2, 3], PlayerAppearanceSeedValues.HairColors
                    .Where(value => value.PlayerRaceId == raceId && value.PlayerSexId == sexId)
                    .Select(value => value.Id));
            }
        }
    }
}
