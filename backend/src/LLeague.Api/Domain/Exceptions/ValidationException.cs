namespace LLeague.Api.Domain.Exceptions;

public sealed class ValidationException(string message) : DomainException(message);
