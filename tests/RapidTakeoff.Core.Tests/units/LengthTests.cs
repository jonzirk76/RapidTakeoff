using RapidTakeoff.Core.Units;
using Xunit;

namespace RapidTakeoff.Core.Tests.Units;

public sealed class LengthTests
{
    [Fact]
    public void FromFeet_ConvertsToInches()
    {
        var len = Length.FromFeet(1);
        Assert.Equal(12.0, len.TotalInches, 10);
    }

    [Fact]
    public void FromFeetAndInches_CombinesCorrectly()
    {
        var len = Length.FromFeetAndInches(5, 6);
        Assert.Equal(66.0, len.TotalInches, 10); // 5*12 + 6
    }

    [Fact]
    public void TotalFeet_IsDerivedFromInches()
    {
        var len = Length.FromInches(30);
        Assert.Equal(2.5, len.TotalFeet, 10);
    }

    [Fact]
    public void Addition_AddsInches()
    {
        var a = Length.FromInches(10);
        var b = Length.FromInches(2.5);
        var c = a + b;

        Assert.Equal(12.5, c.TotalInches, 10);
    }

    [Fact]
    public void Subtraction_CannotGoNegative()
    {
        var a = Length.FromInches(10);
        var b = Length.FromInches(11);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = a - b);
    }

    [Fact]
    public void FromInches_NegativeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = Length.FromInches(-0.01));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void FromInches_NonFiniteThrows(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = Length.FromInches(value));
    }

    [Fact]
    public void Division_ByZeroThrows()
    {
        var a = Length.FromInches(10);
        Assert.Throws<DivideByZeroException>(() => _ = a / 0);
    }
}
