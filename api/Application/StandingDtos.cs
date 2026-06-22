namespace LLeague.Api.Application;

public record StandingRow(
    int Rank, Guid TeamId, int TeamNumber, string TeamName,
    int BestScore, int TotalScore, int MatchesPlayed);

