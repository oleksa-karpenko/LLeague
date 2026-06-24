using LLeague.Api.Application;
using LLeague.Api.Domain.Exceptions;

namespace LLeague.Api.Middleware;

/// <summary>
/// Translates domain exceptions into HTTP responses so controllers/handlers can throw
/// instead of plumbing status codes by hand. Non-domain exceptions are left to bubble
/// up (default 500), so genuine bugs aren't masked.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteError(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteError(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteError(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (DomainException ex)   // ValidationException + any other domain rule
        {
            await WriteError(context, StatusCodes.Status400BadRequest, ex.Message);
        }
    }

    private static Task WriteError(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ErrorResponse(message));
    }
}
