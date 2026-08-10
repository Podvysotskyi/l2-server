namespace L2.Server.Services.Interfaces;

public sealed record GameSessionIssue(string AccessToken, GameSessionState Session, int IdleTimeoutSeconds);
