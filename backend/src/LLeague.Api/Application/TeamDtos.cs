namespace LLeague.Api.Application;

public record TeamRequest(int Number, string Name, string Affiliation, string City, string Region);
public record TeamResponse(Guid Id, int Number, string Name, string Affiliation, string City, string Region);
