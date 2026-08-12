using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public interface ICharacterCreationContentProvider
{
    Task<CharacterCreationOptions> GetAsync(string gameVersion, CancellationToken cancellationToken = default);
}
