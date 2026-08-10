namespace L2.Server.Repositories;

public sealed class PlayerIdentityPersistenceException(string message, Exception innerException)
    : Exception(message, innerException);
