using LLeague.Api.Domain;
using Xunit;

namespace LLeague.Api.Tests.Domain;

/// <summary>Unit tests for the scoresheet apply/submit behavior now on the entity.</summary>
public class ScoresheetDomainTests
{
    private readonly ScoringService _scoring = new();

    private static MissionValue Mission(string id, string type, string raw) =>
        new() { MissionId = id, ClauseIndex = 0, ValueType = type, ValueRaw = raw };

    [Fact]
    public void Apply_scores_server_side_and_drafts_from_empty()
    {
        var sheet = new Scoresheet();   // defaults to Empty

        sheet.Apply([Mission("M01", "boolean", "true"), Mission("M02", "number", "6")], _scoring);

        Assert.Equal(ScoresheetStatus.Draft, sheet.Status);
        Assert.Equal(40, sheet.Score);   // 10 + 30
        Assert.Equal(2, sheet.Missions.Count);
    }

    [Fact]
    public void Apply_only_promotes_status_from_empty()
    {
        var sheet = new Scoresheet { Status = ScoresheetStatus.Completed };

        sheet.Apply([], _scoring);

        Assert.Equal(ScoresheetStatus.Completed, sheet.Status);   // not knocked back to Draft
        Assert.Equal(0, sheet.Score);
    }

    [Fact]
    public void Submit_recomputes_and_finalizes()
    {
        var sheet = new Scoresheet();
        sheet.Apply([Mission("M02", "number", "6")], _scoring);   // Draft, 30

        sheet.Submit(_scoring);

        Assert.Equal(ScoresheetStatus.Completed, sheet.Status);
        Assert.Equal(30, sheet.Score);
    }
}
