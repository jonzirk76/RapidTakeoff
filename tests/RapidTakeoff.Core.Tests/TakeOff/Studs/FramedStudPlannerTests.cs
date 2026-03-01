using RapidTakeoff.Core.Takeoff.Studs;
using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Studs;

public sealed class FramedStudPlannerTests
{
    [Fact]
    public void BuildWallPlan_DoorOpening_ComputesExpectedFramingCounts()
    {
        var plan = FramedStudPlanner.BuildWallPlan(
            wallLength: Length.FromFeet(24.0),
            wallHeight: Length.FromFeet(10.0),
            spacing: Length.FromInches(16.0),
            studWidth: Length.FromFeet(5.5 / 12.0),
            openings:
            [
                new StudOpening(
                    X: Length.FromFeet(2.5),
                    Y: Length.FromFeet(0.0),
                    Width: Length.FromFeet(3.0),
                    Height: Length.FromFeet(7.0))
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
            wallLength: Length.FromFeet(24.0),
            wallHeight: Length.FromFeet(10.0),
            spacing: Length.FromInches(16.0),
            studWidth: Length.FromFeet(5.5 / 12.0),
            openings:
            [
                new StudOpening(
                    X: Length.FromFeet(8.0),
                    Y: Length.FromFeet(3.0),
                    Width: Length.FromFeet(4.0),
                    Height: Length.FromFeet(3.0))
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
            wallLength: Length.FromFeet(24.0),
            wallHeight: Length.FromFeet(10.0),
            spacing: Length.FromInches(16.0),
            studWidth: Length.FromFeet(5.5 / 12.0),
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
