namespace LLeague.Api.Application;

public record SeasonRequest(string Name, int Year);
public record SeasonResponse(Guid Id, string Name, int Year);
