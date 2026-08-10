using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public sealed record AuthenticationSessionLookup(AuthenticationSession Session, bool Refreshed);
