using RapidTakeoff.Core.Takeoff.Studs;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Studs;

public sealed class FramedStudPlannerTests
{
    [Fact]
    public void BuildWallPlan_DoorOpening_ComputesExpectedFramingCounts()
    {
        var plan = FramedStudPlanner.BuildWallPlan(
            wallLengthFeet: 24.0,
            wallHeightFeet: 10.0,
            spacingInches: 16.0,
            studWidthFeet: 5.5 / 12.0,
            openings:
            [
                new StudOpening(
                    XFeet: 2.5,
                    YFeet: 0.0,
                    WidthFeet: 3.0,
                    HeightFeet: 7.0)
            ]);

        Assert.Equal(19, plan.NominalCenters.Count);
        Assert.Equal(16, plan.CommonCenters.Count);
        Assert.Equal(2, plan.KingCenters.Count);
        Assert.Equal(18, plan.FinalCenters.Count);
        Assert.Equal(2, plan.TrimmerCount);
        Assert.Equal(3, plan.CrippleTopCount);
        Assert.Equal(0, plan.CrippleBottomCount);
        Assert.Equal(23, plan.BaseStudCount);
    }

    [Fact]
    public void BuildWallPlan_WindowOpening_ComputesSillAndCrippleCounts()
    {
        var plan = FramedStudPlanner.BuildWallPlan(
            wallLengthFeet: 24.0,
            wallHeightFeet: 10.0,
            spacingInches: 16.0,
            studWidthFeet: 5.5 / 12.0,
            openings:
            [
                new StudOpening(
                    XFeet: 8.0,
                    YFeet: 3.0,
                    WidthFeet: 4.0,
                    HeightFeet: 3.0)
            ]);

        Assert.Equal(19, plan.NominalCenters.Count);
        Assert.Equal(15, plan.CommonCenters.Count);
        Assert.Equal(2, plan.KingCenters.Count);
        Assert.Equal(17, plan.FinalCenters.Count);
        Assert.Equal(2, plan.TrimmerCount);
        Assert.Equal(2, plan.CrippleTopCount);
        Assert.Equal(2, plan.CrippleBottomCount);
        Assert.Equal(23, plan.BaseStudCount);
    }

    [Fact]
    public void BuildWallPlan_NoOpenings_ReturnsNominalOnly()
    {
        var plan = FramedStudPlanner.BuildWallPlan(
            wallLengthFeet: 24.0,
            wallHeightFeet: 10.0,
            spacingInches: 16.0,
            studWidthFeet: 5.5 / 12.0,
            openings: []);

        Assert.Equal(19, plan.NominalCenters.Count);
        Assert.Equal(plan.NominalCenters.Count, plan.CommonCenters.Count);
        Assert.Empty(plan.KingCenters);
        Assert.Equal(plan.NominalCenters.Count, plan.FinalCenters.Count);
        Assert.Equal(0, plan.TrimmerCount);
        Assert.Equal(0, plan.CrippleTopCount);
        Assert.Equal(0, plan.CrippleBottomCount);
        Assert.Equal(19, plan.BaseStudCount);
    }
}
