namespace LLeague.Api.Application;

public record TableRequest(string Name);
public record TableResponse(Guid Id, string Name);
