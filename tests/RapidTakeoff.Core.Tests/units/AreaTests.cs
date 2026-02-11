using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Units;

public sealed class AreaTests
{
    [Fact]
    public void FromSquareFeet_ConvertsToSquareInches()
    {
        var a = Area.FromSquareFeet(1);
        Assert.Equal(144.0, a.TotalSquareInches, 10);
    }

    [Fact]
    public void TotalSquareFeet_IsDerivedFromSquareInches()
    {
        var a = Area.FromSquareInches(288);
        Assert.Equal(2.0, a.TotalSquareFeet, 10);
    }

    [Fact]
    public void Addition_AddsSquareInches()
    {
        var a = Area.FromSquareInches(100);
        var b = Area.FromSquareInches(44);
        var c = a + b;

        Assert.Equal(144.0, c.TotalSquareInches, 10);
    }

    [Fact]
    public void Subtraction_CannotGoNegative()
    {
        var a = Area.FromSquareInches(10);
        var b = Area.FromSquareInches(11);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = a - b);
    }

    [Fact]
    public void FromSquareFeet_NegativeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = Area.FromSquareFeet(-0.01));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void FromSquareInches_NonFiniteThrows(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = Area.FromSquareInches(value));
    }

    [Fact]
    public void Division_ByZeroThrows()
    {
        var a = Area.FromSquareInches(10);
        Assert.Throws<DivideByZeroException>(() => _ = a / 0);
    }
}
