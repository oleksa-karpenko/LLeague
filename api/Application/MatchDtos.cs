namespace LLeague.Api.Application;

public record MatchParticipantRequest(Guid TableId, Guid TeamId);
public record MatchRequest(int Round, int Number, string Stage, List<MatchParticipantRequest> Participants);

public record MatchParticipantResponse(
    Guid Id, Guid TableId, string TableName, Guid TeamId, int TeamNumber, string TeamName,
    bool Ready, string ScoresheetStatus, int Score);

public record MatchResponse(
    Guid Id, int Round, int Number, string Stage, string Status,
    DateTimeOffset? ScheduledTime, DateTimeOffset? StartTime,
    List<MatchParticipantResponse> Participants);
