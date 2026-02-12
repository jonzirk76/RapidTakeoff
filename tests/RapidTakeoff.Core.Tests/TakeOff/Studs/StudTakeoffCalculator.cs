using RapidTakeoff.Core.Takeoff.Studs;
using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Studs;

public sealed class StudTakeoffCalculatorTests
{
    [Fact]
    public void Calculate_OneWall_8ft_16oc_Returns7()
    {
        var walls = new[] { Length.FromFeet(8) };     // 96 in
        var spacing = Length.FromInches(16);          // 16 in

        var result = StudTakeoffCalculator.Calculate(walls, spacing);

        Assert.Equal(7, result.BaseStuds);            // ceil(96/16)+1 = 6+1
        Assert.Equal(7, result.TotalStuds);
        Assert.Single(result.StudsPerWall);
        Assert.Equal(7, result.StudsPerWall[0]);
    }

    [Fact]
    public void Calculate_OneWall_8ft_24oc_Returns5()
    {
        var walls = new[] { Length.FromFeet(8) };     // 96 in
        var spacing = Length.FromInches(24);          // 24 in

        var result = StudTakeoffCalculator.Calculate(walls, spacing);

        Assert.Equal(5, result.TotalStuds);           // ceil(96/24)+1 = 4+1
    }

    [Fact]
    public void Calculate_MultipleWalls_SumsPerWall()
    {
        var walls = new[] { Length.FromFeet(12), Length.FromFeet(10) };
        var spacing = Length.FromInches(16);

        // 12ft = 144in => ceil(144/16)+1 = 9+1=10
        // 10ft = 120in => ceil(120/16)+1 = 8+1=9
        var result = StudTakeoffCalculator.Calculate(walls, spacing);

        Assert.Equal(19, result.BaseStuds);
        Assert.Equal(new[] { 10, 9 }, result.StudsPerWall);
    }

    [Fact]
    public void Calculate_WithWaste_CeilingsTotal()
    {
        var walls = new[] { Length.FromFeet(8) }; // base 7
        var spacing = Length.FromInches(16);

        var result = StudTakeoffCalculator.Calculate(walls, spacing, 0.10);

        Assert.Equal(7, result.BaseStuds);
        Assert.Equal(8, result.TotalStuds); // ceil(7*1.10)=ceil(7.7)=8
    }

    [Fact]
    public void Calculate_InvalidSpacing_Throws()
    {
        var walls = new[] { Length.FromFeet(8) };
        var spacing = Length.FromInches(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => StudTakeoffCalculator.Calculate(walls, spacing));
    }

    [Fact]
    public void Calculate_EmptyWalls_Throws()
    {
        var spacing = Length.FromInches(16);
        Assert.Throws<ArgumentOutOfRangeException>(() => StudTakeoffCalculator.Calculate(Array.Empty<Length>(), spacing));
    }

    [Fact]
    public void Calculate_NegativeWaste_Throws()
    {
        var walls = new[] { Length.FromFeet(8) };
        var spacing = Length.FromInches(16);

        Assert.Throws<ArgumentOutOfRangeException>(() => StudTakeoffCalculator.Calculate(walls, spacing, -0.01));
    }
}
