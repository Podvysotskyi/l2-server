namespace L2.Server.Contracts;

public sealed record LoginRequest(string Email, string Password, string GameVersion);
