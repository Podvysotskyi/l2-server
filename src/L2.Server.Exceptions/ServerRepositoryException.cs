namespace L2.Server.Exceptions;

public sealed class ServerRepositoryException(string message, Exception innerException)
    : Exception(message, innerException);
