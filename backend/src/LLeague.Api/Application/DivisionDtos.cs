namespace LLeague.Api.Application;

public record DivisionRequest(Guid EventId, string Name, string Color);
public record DivisionResponse(Guid Id, Guid EventId, string Name, string Color);
