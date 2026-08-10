using L2.Server.Contracts;

namespace L2.Server.Repositories.Interfaces;

public sealed record AuthenticationSessionRecord(AuthenticationSession Session, bool Refreshed);
