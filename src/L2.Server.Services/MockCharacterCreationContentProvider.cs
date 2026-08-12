using L2.Server.Contracts;
using L2.Server.Services.Interfaces;

namespace L2.Server.Services;

public sealed class MockCharacterCreationContentProvider : ICharacterCreationContentProvider
{
    private const int HumanRaceId = 0;
    private const int MaleSexId = 0;
    private const int FemaleSexId = 1;

    private static readonly CharacterCreationOptions CreationOptions = new(0,
    [
        RootClass(0, "Human Fighter", false),
        RootClass(10, "Human Mystic", true)
    ]);

    public Task<CharacterCreationOptions> GetAsync(
        string gameVersion,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: Replace this mock with a Redis-backed provider after Studio publishes character-creation content.
        return Task.FromResult(CreationOptions);
    }

    private static RootClassOption RootClass(int id, string name, bool isMage) => new(
        id,
        name,
        isMage,
        [
            new RaceOption(HumanRaceId, "Human",
            [
                Sex(MaleSexId, "Male", 3, 5, 4),
                Sex(FemaleSexId, "Female", 7, 7, 4)
            ])
        ]);

    private static SexOption Sex(
        int id,
        string name,
        int faceCount,
        int hairStyleCount,
        int hairColorCount) => new(
        id,
        name,
        Appearance(faceCount),
        Appearance(hairStyleCount),
        Appearance(hairColorCount));

    private static AppearanceOption[] Appearance(int count) =>
        Enumerable.Range(0, count)
            .Select(id => new AppearanceOption(id, $"Option {id + 1}"))
            .ToArray();
}
