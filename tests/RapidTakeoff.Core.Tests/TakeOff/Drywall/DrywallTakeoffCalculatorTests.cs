using RapidTakeoff.Core.Takeoff.Drywall;
using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Drywall;

public sealed class DrywallTakeoffCalculatorTests
{
    [Fact]
    public void Calculate_ZeroArea_ReturnsZeroSheets()
    {
        var net = Area.FromSquareFeet(0);
        var result = DrywallTakeoffCalculator.Calculate(net, DrywallSheet.Sheet4x8, 0.10);

        Assert.Equal(0, result.SheetCount);
    }

    [Fact]
    public void Calculate_NoWaste_ExactMultiple_ReturnsExactSheets()
    {
        // 64 sqft, 4x8 sheets are 32 sqft => exactly 2
        var net = Area.FromSquareFeet(64);
        var result = DrywallTakeoffCalculator.Calculate(net, DrywallSheet.Sheet4x8, 0);

        Assert.Equal(2, result.SheetCount);
        Assert.Equal(64.0, result.GrossArea.TotalSquareFeet, 10);
    }

    [Fact]
    public void Calculate_WithWaste_CeilingsSheets()
    {
        // 60 sqft with 10% waste => 66 sqft
        // 66/32 = 2.0625 => ceil => 3 sheets
        var net = Area.FromSquareFeet(60);
        var result = DrywallTakeoffCalculator.Calculate(net, DrywallSheet.Sheet4x8, 0.10);

        Assert.Equal(3, result.SheetCount);
        Assert.Equal(66.0, result.GrossArea.TotalSquareFeet, 10);
    }

    [Fact]
    public void Calculate_NegativeWaste_Throws()
    {
        var net = Area.FromSquareFeet(10);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _ = DrywallTakeoffCalculator.Calculate(net, DrywallSheet.Sheet4x8, -0.01));
    }
}
