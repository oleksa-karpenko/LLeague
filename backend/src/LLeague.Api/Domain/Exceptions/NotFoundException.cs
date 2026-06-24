namespace LLeague.Api.Domain.Exceptions;

public sealed class NotFoundException(string message = "Not found") : DomainException(message);
