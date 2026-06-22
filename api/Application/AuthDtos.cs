namespace LLeague.Api.Application;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);
public record ErrorResponse(string Error);
