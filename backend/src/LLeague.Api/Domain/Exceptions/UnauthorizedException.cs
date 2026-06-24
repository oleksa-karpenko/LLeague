namespace LLeague.Api.Domain.Exceptions;

public sealed class UnauthorizedException(string message = "Unauthorized") : DomainException(message);
