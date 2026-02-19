using RapidTakeoff.Core.Takeoff.Studs;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Studs;

public sealed class StudLayoutPlannerTests
{
    [Fact]
    public void GenerateStudCentersFeet_8ftAt16Oc_MatchesExpectedCenters()
    {
        var centers = StudLayoutPlanner.GenerateStudCentersFeet(8.0, 16.0);

        Assert.Equal(7, centers.Count);
        Assert.Equal(0.0, centers[0], 10);
        Assert.Equal(8.0, centers[^1], 10);
    }

    [Fact]
    public void RemoveCentersInsideSpans_RemovesCentersOnSpanEdges()
    {
        var centers = StudLayoutPlanner.GenerateStudCentersFeet(8.0, 16.0);
        var spans = new[]
        {
            new LinearSpan(1.3333333333, 4.0)
        };

        var filtered = StudLayoutPlanner.RemoveCentersInsideSpans(centers, spans);

        Assert.DoesNotContain(filtered, x => Math.Abs(x - 1.3333333333) < 1e-6);
        Assert.DoesNotContain(filtered, x => Math.Abs(x - 4.0) < 1e-6);
        Assert.Contains(filtered, x => Math.Abs(x - 0.0) < 1e-6);
        Assert.Contains(filtered, x => Math.Abs(x - 5.3333333333) < 1e-6);
    }

    [Fact]
    public void RemoveCentersInsideSpans_OverlappingSpans_RemovesUnion()
    {
        var centers = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
        var spans = new[]
        {
            new LinearSpan(1.5, 3.5),
            new LinearSpan(2.5, 4.5)
        };

        var filtered = StudLayoutPlanner.RemoveCentersInsideSpans(centers, spans);

        Assert.Equal(new[] { 0.0, 1.0 }, filtered);
    }

    [Fact]
    public void AddKingStudCenters_AddsSideCentersAtHalfStudWidth()
    {
        var centers = new[] { 0.0, 6.0, 12.0 };
        var openings = new[] { new LinearSpan(4.0, 8.0) };
        var studWidthFeet = 0.5; // 6 in width => +/- 0.25 ft

        var withKings = StudLayoutPlanner.AddKingStudCenters(centers, openings, studWidthFeet, wallLengthFeet: 12.0);

        Assert.Contains(withKings, x => Math.Abs(x - 3.75) < 1e-6);
        Assert.Contains(withKings, x => Math.Abs(x - 8.25) < 1e-6);
    }

    [Fact]
    public void AddKingStudCenters_ClampsToWallBoundsAndDedupes()
    {
        var centers = new[] { 0.0, 12.0 };
        var openings = new[]
        {
            new LinearSpan(0.2, 1.0),   // left king outside wall
            new LinearSpan(10.8, 11.8)  // right king outside wall
        };
        var studWidthFeet = 1.0;

        var withKings = StudLayoutPlanner.AddKingStudCenters(centers, openings, studWidthFeet, wallLengthFeet: 12.0);

        Assert.Equal(withKings.OrderBy(x => x).ToArray(), withKings);
        Assert.Single(withKings.Where(x => Math.Abs(x - 0.0) < 1e-6));
        Assert.Single(withKings.Where(x => Math.Abs(x - 12.0) < 1e-6));
    }
}
