namespace LLeague.Api.Application;

public record MissionValueDto(string MissionId, int ClauseIndex, string ValueType, string? Value);
public record ScoresheetUpdateRequest(List<MissionValueDto> Missions);
public record ScoresheetResponse(Guid ParticipantId, string Status, int Score, List<MissionValueDto> Missions);
