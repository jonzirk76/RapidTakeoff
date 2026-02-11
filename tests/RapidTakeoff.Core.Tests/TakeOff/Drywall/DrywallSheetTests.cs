using RapidTakeoff.Core.Takeoff.Drywall;
using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Takeoff.Drywall;

public sealed class DrywallSheetTests
{
    [Fact]
    public void Sheet4x8_HasExpectedArea()
    {
        var s = DrywallSheet.Sheet4x8;
        Assert.Equal(32.0, s.Area.TotalSquareFeet, 10);
    }

    [Fact]
    public void Sheet4x12_HasExpectedArea()
    {
        var s = DrywallSheet.Sheet4x12;
        Assert.Equal(48.0, s.Area.TotalSquareFeet, 10);
    }

    [Fact]
    public void FromDimensions_ZeroOrNegativeThrows()
    {
        var ok = Length.FromFeet(4);
        var zero = Length.FromInches(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = DrywallSheet.FromDimensions(zero, ok));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = DrywallSheet.FromDimensions(ok, zero));
    }
}
