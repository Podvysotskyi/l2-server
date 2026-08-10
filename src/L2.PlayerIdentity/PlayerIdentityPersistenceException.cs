namespace L2.PlayerIdentity;

public sealed class PlayerIdentityPersistenceException(string message, Exception innerException)
    : Exception(message, innerException);
