namespace LLeague.Api.Application;

public record EnrollRequest(Guid TeamId);

public record EnrolledTeamResponse(
    Guid TeamId,
    int Number,
    string Name,
    string Affiliation,
    string City,
    string Region,
    bool Arrived,
    DateTimeOffset? ArrivedAt);
