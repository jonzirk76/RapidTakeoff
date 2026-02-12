using RapidTakeoff.Core.Takeoff.Insulation;
using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Insulation;

public sealed class InsulationTakeoffCalculatorTests
{
    [Fact]
    public void Calculate_ZeroArea_ReturnsZeroQuantity()
    {
        var net = Area.FromSquareFeet(0);
        var product = new InsulationProduct(Area.FromSquareFeet(40), "R13 Batts");

        var result = InsulationTakeoffCalculator.Calculate(net, product, 0.10);

        Assert.Equal(0, result.Quantity);
    }

    [Fact]
    public void Calculate_UsesAreaConversionAndCeilingsQuantity()
    {
        // 600 sqft * 1.10 = 660 sqft gross, / 40 sqft coverage = 16.5 -> 17
        var net = Area.FromSquareFeet(600);
        var product = new InsulationProduct(Area.FromSquareFeet(40), "R13 Batts");

        var result = InsulationTakeoffCalculator.Calculate(net, product, 0.10);

        Assert.Equal(600.0, result.NetArea.TotalSquareFeet, 10);
        Assert.Equal(660.0, result.GrossArea.TotalSquareFeet, 10);
        Assert.Equal(17, result.Quantity);
    }

    [Fact]
    public void Calculate_NoWasteExactMultiple_ReturnsExactQuantity()
    {
        var net = Area.FromSquareFeet(160);
        var product = new InsulationProduct(Area.FromSquareFeet(40), "R13 Batts");

        var result = InsulationTakeoffCalculator.Calculate(net, product, 0);

        Assert.Equal(4, result.Quantity);
        Assert.Equal(160.0, result.GrossArea.TotalSquareFeet, 10);
    }

    [Fact]
    public void Calculate_NegativeWaste_Throws()
    {
        var net = Area.FromSquareFeet(100);
        var product = new InsulationProduct(Area.FromSquareFeet(40), "R13 Batts");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _ = InsulationTakeoffCalculator.Calculate(net, product, -0.01));
    }

    [Fact]
    public void Product_ZeroCoverage_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _ = new InsulationProduct(Area.FromSquareFeet(0), "Invalid Product"));
    }
}
