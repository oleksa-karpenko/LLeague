namespace LLeague.Api.Domain;

public enum MatchStage
{
    Practice,
    Ranking,
    Test
}

public enum MatchStatus
{
    NotStarted,
    InProgress,
    Completed,
}

public enum ScoresheetStatus
{
    Empty,
    Draft,
    Completed,
}
