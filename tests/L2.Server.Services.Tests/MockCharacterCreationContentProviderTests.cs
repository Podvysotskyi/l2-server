namespace L2.Server.Services.Tests;

public sealed class MockCharacterCreationContentProviderTests
{
    [Fact]
    public async Task GetAsync_returns_minimal_fighter_and_mage_content()
    {
        var provider = new MockCharacterCreationContentProvider();

        var options = await provider.GetAsync();

        Assert.Equal(2, options.Classes.Count);
        Assert.Contains(options.Classes, item =>
            item.Id == 0 && item.Name == "Human Fighter" && !item.IsMage);
        Assert.Contains(options.Classes, item =>
            item.Id == 10 && item.Name == "Human Mystic" && item.IsMage);
        Assert.All(options.Classes, rootClass =>
        {
            var race = Assert.Single(rootClass.AllowedRaces);
            Assert.Equal(0, race.Id);
            Assert.Equal("Human", race.Name);
            Assert.Collection(race.AllowedSexes,
                male =>
                {
                    Assert.Equal(0, male.Id);
                    Assert.Equal(3, male.Faces.Count);
                    Assert.Equal(5, male.HairStyles.Count);
                    Assert.Equal(4, male.HairColors.Count);
                },
                female =>
                {
                    Assert.Equal(1, female.Id);
                    Assert.Equal(7, female.Faces.Count);
                    Assert.Equal(7, female.HairStyles.Count);
                    Assert.Equal(4, female.HairColors.Count);
                });
        });
    }

    [Fact]
    public async Task GetAsync_honors_cancellation()
    {
        var provider = new MockCharacterCreationContentProvider();
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => provider.GetAsync(source.Token));
    }
}
