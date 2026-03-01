using RapidTakeoff.Core.Takeoff.Studs;
using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Studs;

public sealed class StudLayoutPlannerTests
{
    [Fact]
    public void GenerateStudCenters_8ftAt16Oc_MatchesExpectedCenters()
    {
        var centers = StudLayoutPlanner.GenerateStudCenters(Length.FromFeet(8.0), Length.FromInches(16.0));

        Assert.Equal(7, centers.Count);
        Assert.Equal(0.0, centers[0].TotalFeet, 10);
        Assert.Equal(8.0, centers[^1].TotalFeet, 10);
    }

    [Fact]
    public void RemoveCentersInsideSpans_RemovesCentersOnSpanEdges()
    {
        var centers = StudLayoutPlanner.GenerateStudCenters(Length.FromFeet(8.0), Length.FromInches(16.0));
        var spans = new[]
        {
            new LinearSpan(Length.FromFeet(1.3333333333), Length.FromFeet(4.0))
        };

        var filtered = StudLayoutPlanner.RemoveCentersInsideSpans(centers, spans);

        Assert.DoesNotContain(filtered, x => Math.Abs(x.TotalFeet - 1.3333333333) < 1e-6);
        Assert.DoesNotContain(filtered, x => Math.Abs(x.TotalFeet - 4.0) < 1e-6);
        Assert.Contains(filtered, x => Math.Abs(x.TotalFeet - 0.0) < 1e-6);
        Assert.Contains(filtered, x => Math.Abs(x.TotalFeet - 5.3333333333) < 1e-6);
    }

    [Fact]
    public void RemoveCentersInsideSpans_OverlappingSpans_RemovesUnion()
    {
        var centers = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 }.Select(Length.FromFeet).ToArray();
        var spans = new[]
        {
            new LinearSpan(Length.FromFeet(1.5), Length.FromFeet(3.5)),
            new LinearSpan(Length.FromFeet(2.5), Length.FromFeet(4.5))
        };

        var filtered = StudLayoutPlanner.RemoveCentersInsideSpans(centers, spans);

        Assert.Equal(new[] { 0.0, 1.0 }, filtered.Select(x => x.TotalFeet).ToArray());
    }

    [Fact]
    public void AddKingStudCenters_AddsSideCentersAtOnePointFiveStudWidths()
    {
        var centers = new[] { 0.0, 6.0, 12.0 }.Select(Length.FromFeet).ToArray();
        var openings = new[] { new LinearSpan(Length.FromFeet(4.0), Length.FromFeet(8.0)) };
        var studWidth = Length.FromFeet(0.5); // 6 in width => king offset +/- 0.75 ft

        var withKings = StudLayoutPlanner.AddKingStudCenters(centers, openings, studWidth, wallLength: Length.FromFeet(12.0));

        Assert.Contains(withKings, x => Math.Abs(x.TotalFeet - 3.25) < 1e-6);
        Assert.Contains(withKings, x => Math.Abs(x.TotalFeet - 8.75) < 1e-6);
    }

    [Fact]
    public void AddKingStudCenters_ClampsToWallBoundsAndDedupes()
    {
        var centers = new[] { 0.0, 12.0 }.Select(Length.FromFeet).ToArray();
        var openings = new[]
        {
            new LinearSpan(Length.FromFeet(0.2), Length.FromFeet(1.0)),   // left king outside wall
            new LinearSpan(Length.FromFeet(10.8), Length.FromFeet(11.8))  // right king outside wall
        };
        var studWidth = Length.FromFeet(1.0);

        var withKings = StudLayoutPlanner.AddKingStudCenters(centers, openings, studWidth, wallLength: Length.FromFeet(12.0));

        Assert.Equal(withKings.OrderBy(x => x.TotalInches).ToArray(), withKings);
        Assert.Single(withKings.Where(x => Math.Abs(x.TotalFeet - 0.0) < 1e-6));
        Assert.Single(withKings.Where(x => Math.Abs(x.TotalFeet - 12.0) < 1e-6));
    }

    [Fact]
    public void RemoveFramedOpeningZone_ThenAddKings_LeavesOnlyKingsNearOpening()
    {
        var generated = StudLayoutPlanner.GenerateStudCenters(Length.FromFeet(12.0), Length.FromInches(16.0));
        var studWidth = Length.FromFeet(0.5);
        var opening = new LinearSpan(Length.FromFeet(4.0), Length.FromFeet(8.0));
        var framedZone = new LinearSpan(Length.FromFeet(4.0 - (1.5 * studWidth.TotalFeet)), Length.FromFeet(8.0 + (1.5 * studWidth.TotalFeet)));

        var trimmed = StudLayoutPlanner.RemoveCentersInsideSpans(generated, new[] { framedZone });
        var final = StudLayoutPlanner.AddKingStudCenters(trimmed, new[] { opening }, studWidth, Length.FromFeet(12.0));

        Assert.Contains(final, x => Math.Abs(x.TotalFeet - 3.25) < 1e-6);
        Assert.Contains(final, x => Math.Abs(x.TotalFeet - 8.75) < 1e-6);
        Assert.DoesNotContain(final, x => x.TotalFeet > 3.25 + 1e-6 && x.TotalFeet < 8.75 - 1e-6);
    }
}
