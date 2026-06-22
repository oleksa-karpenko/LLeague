using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Xunit;

namespace LLeague.Api.Tests.Domain;

/// <summary>Unit tests for the Match state machine now encapsulated on the entity.</summary>
public class MatchDomainTests
{
    [Fact]
    public void Start_from_not_started_moves_to_in_progress_and_stamps_time()
    {
        var m = new Match();

        m.Start();

        Assert.Equal(MatchStatus.InProgress, m.Status);
        Assert.NotNull(m.StartTime);
    }

    [Fact]
    public void Complete_from_in_progress_moves_to_completed()
    {
        var m = new Match();
        m.Start();

        m.Complete();

        Assert.Equal(MatchStatus.Completed, m.Status);
    }

    [Fact]
    public void Abort_resets_to_not_started_and_clears_start_time()
    {
        var m = new Match();
        m.Start();

        m.Abort();

        Assert.Equal(MatchStatus.NotStarted, m.Status);
        Assert.Null(m.StartTime);
    }

    [Fact]
    public void Complete_without_start_throws_conflict()
    {
        var m = new Match();

        Assert.Throws<ConflictException>(m.Complete);
    }

    [Fact]
    public void Start_twice_throws_conflict()
    {
        var m = new Match();
        m.Start();

        Assert.Throws<ConflictException>(m.Start);
    }
}
