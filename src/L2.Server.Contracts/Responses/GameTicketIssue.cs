namespace L2.Server.Contracts;

public sealed record GameTicketIssue(string Ticket, DateTimeOffset ExpiresAt);
