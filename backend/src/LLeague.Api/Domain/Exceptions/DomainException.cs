namespace LLeague.Api.Domain.Exceptions;

/// <summary>
/// Base type for domain rule violations. ExceptionHandlingMiddleware maps subtypes to
/// status codes: NotFoundException -> 404, UnauthorizedException -> 401,
/// ConflictException -> 409, anything else -> 400.
/// </summary>
public class DomainException(string message) : Exception(message);
