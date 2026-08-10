using L2.GameContent.Identifiers;
using L2.GameContent.Seeding;
using Xunit;

namespace L2.Foundation.Tests;

public sealed class SkillSeedValuesTests
{
    [Fact]
    public void Catalog_contains_all_direct_skill_definitions_and_concrete_icons()
    {
        Assert.Equal(2_700, SkillSeedValues.Skills.Count);
        Assert.Equal(11_392, SkillSeedValues.Icons.Count);
        Assert.Equal(7, SkillSeedValues.OperateTypes.Count);
        Assert.Equal(27, SkillSeedValues.TargetTypes.Count);
        Assert.Equal(
            Enum.GetValues<SkillOperateTypeId>(),
            SkillSeedValues.OperateTypes.Select(value => value.Id));
        Assert.Equal(
            Enum.GetValues<SkillTargetTypeId>(),
            SkillSeedValues.TargetTypes.Select(value => value.Id));
        Assert.Equal(SkillSeedValues.Skills.Count, SkillSeedValues.Skills.Select(value => value.Id).Distinct().Count());
        Assert.Equal(
            SkillSeedValues.Icons.Count,
            SkillSeedValues.Icons.Select(value => (value.SkillId, value.Level)).Distinct().Count());
        Assert.Equal(
            SkillSeedValues.OperateTypes.Count,
            SkillSeedValues.OperateTypes.Select(value => value.Name).Distinct().Count());
        Assert.Equal(
            SkillSeedValues.TargetTypes.Count,
            SkillSeedValues.TargetTypes.Select(value => value.Name).Distinct().Count());
        Assert.All(SkillSeedValues.Skills, value => Assert.InRange(value.Levels, (short)1, (short)80));
        Assert.All(SkillSeedValues.Skills, value => Assert.True(value.Name.Length <= 100));
        Assert.All(SkillSeedValues.Icons, value => Assert.True(value.Name.Length <= 64));
        Assert.All(SkillSeedValues.Icons, value => Assert.InRange(value.Level, (short)1, (short)80));
        Assert.DoesNotContain(SkillSeedValues.Icons, value => value.Level == 0);

        var skillIds = SkillSeedValues.Skills.Select(value => value.Id).ToHashSet();
        var operateTypeIds = SkillSeedValues.OperateTypes.Select(value => value.Id).ToHashSet();
        var targetTypeIds = SkillSeedValues.TargetTypes.Select(value => value.Id).ToHashSet();
        Assert.All(SkillSeedValues.Icons, value => Assert.Contains(value.SkillId, skillIds));
        Assert.All(
            SkillSeedValues.Skills,
            value => Assert.Contains(value.SkillOperateTypeId!.Value, operateTypeIds));
        Assert.All(
            SkillSeedValues.Skills,
            value => Assert.Contains(value.SkillTargetTypeId!.Value, targetTypeIds));

        var tripleSlash = Assert.Single(SkillSeedValues.Skills, value => value.Id == 1);
        Assert.Equal(37, tripleSlash.Levels);
        Assert.Equal("Triple Slash", tripleSlash.Name);
        Assert.Equal(SkillOperateTypeId.A1, tripleSlash.SkillOperateTypeId);
        Assert.Equal(SkillTargetTypeId.One, tripleSlash.SkillTargetTypeId);
        var tripleSlashIcons = SkillSeedValues.Icons.Where(value => value.SkillId == 1).ToArray();
        Assert.Equal(37, tripleSlashIcons.Length);
        Assert.Equal(Enumerable.Range(1, 37), tripleSlashIcons.Select(value => (int)value.Level));
        Assert.All(tripleSlashIcons, icon => Assert.Equal("icon.skill0001", icon.Name));
        Assert.Equal(
            "A1",
            Assert.Single(SkillSeedValues.OperateTypes, value => value.Id == tripleSlash.SkillOperateTypeId).Name);
        Assert.Equal(
            "ONE",
            Assert.Single(SkillSeedValues.TargetTypes, value => value.Id == tripleSlash.SkillTargetTypeId).Name);

        var expertiseIcons = SkillSeedValues.Icons.Where(value => value.SkillId == 239).ToArray();
        Assert.Equal([1, 2, 3, 4, 5], expertiseIcons.Select(value => (int)value.Level));
    }
}
