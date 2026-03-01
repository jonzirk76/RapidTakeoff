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

    [Theory]
    [InlineData(UnitBasis.Inches, 24.0)]
    [InlineData(UnitBasis.Feet, 2.0)]
    [InlineData(UnitBasis.Millimeters, 609.6)]
    [InlineData(UnitBasis.Centimeters, 60.96)]
    [InlineData(UnitBasis.Meters, 0.6096)]
    public void To_ConvertsToRequestedBasis(UnitBasis basis, double expected)
    {
        var len = Length.FromInches(24);
        Assert.Equal(expected, len.To(basis), 10);
    }

    [Theory]
    [InlineData(UnitBasis.Inches, 24.0)]
    [InlineData(UnitBasis.Feet, 2.0)]
    [InlineData(UnitBasis.Millimeters, 609.6)]
    [InlineData(UnitBasis.Centimeters, 60.96)]
    [InlineData(UnitBasis.Meters, 0.6096)]
    public void From_ConvertsFromRequestedBasis(UnitBasis basis, double input)
    {
        var len = Length.From(input, basis);
        Assert.Equal(24.0, len.TotalInches, 10);
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

    [Theory]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void FromFeet_RejectsInvalid(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = Length.FromFeet(value));
    }

    [Theory]
    [InlineData(-0.01, 0)]
    [InlineData(0, -0.01)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.NaN)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(0, double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity, 0)]
    [InlineData(0, double.NegativeInfinity)]
    public void FromFeetAndInches_RejectsInvalid(double feet, double inches)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = Length.FromFeetAndInches(feet, inches));
    }

    [Theory]
    [InlineData(UnitBasis.Inches, -0.01)]
    [InlineData(UnitBasis.Feet, -0.01)]
    [InlineData(UnitBasis.Millimeters, -0.01)]
    [InlineData(UnitBasis.Centimeters, -0.01)]
    [InlineData(UnitBasis.Meters, -0.01)]
    [InlineData(UnitBasis.Inches, double.NaN)]
    [InlineData(UnitBasis.Feet, double.PositiveInfinity)]
    [InlineData(UnitBasis.Millimeters, double.NegativeInfinity)]
    public void From_WithBasis_RejectsInvalid(UnitBasis basis, double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = Length.From(value, basis));
    }

    [Fact]
    public void Division_ByZeroThrows()
    {
        var a = Length.FromInches(10);
        Assert.Throws<DivideByZeroException>(() => _ = a / 0);
    }

    [Theory]
    [InlineData(1.0 / 32.0, "0'-1/16\"")]
    [InlineData(5.5, "0'-5 1/2\"")]
    [InlineData(11.999, "1'-0\"")]
    [InlineData(12.0, "1'-0\"")]
    [InlineData(66.25, "5'-6 1/4\"")]
    public void Format_Architectural_RoundsCarriesAndReduces(double inches, string expected)
    {
        var len = Length.FromInches(inches);
        Assert.Equal(expected, len.Format(UnitStyle.Architectural));
    }

    [Theory]
    [InlineData(11.999, 2, "1'-0\"")]
    [InlineData(66.25, 2, "5'-6.25\"")]
    [InlineData(66.2, 0, "5'-6\"")]
    public void Format_Engineering_RoundsAndCarries(double inches, int precision, string expected)
    {
        var len = Length.FromInches(inches);
        Assert.Equal(expected, len.Format(UnitStyle.Engineering, precision: precision));
    }

    [Fact]
    public void Format_Decimal_UsesRequestedBasis()
    {
        var len = Length.FromInches(12);
        Assert.Equal("304.8 mm", len.Format(UnitStyle.Decimal, UnitBasis.Millimeters, precision: 3));
    }

    [Fact]
    public void Format_Scientific_UsesRequestedBasis()
    {
        var len = Length.FromInches(12);
        Assert.Equal("3.048E+002 mm", len.Format(UnitStyle.Scientific, UnitBasis.Millimeters, precision: 3));
    }

    [Theory]
    [InlineData("5' 6 1/4\"", 66.25)]
    [InlineData("5'-6 1/4\"", 66.25)]
    [InlineData("5'", 60.0)]
    [InlineData("66.25 in", 66.25)]
    [InlineData("5.5208333333 ft", 66.25)]
    [InlineData("1682.75 mm", 66.25)]
    [InlineData("168.275 cm", 66.25)]
    [InlineData("1.68275 m", 66.25)]
    [InlineData("5 ft 6.25 in", 66.25)]
    [InlineData("66.25", 66.25)]
    [InlineData("1/3\"", 0.3125)] // parse normalization to nearest 1/16
    public void Parse_ParsesSupportedFormats(string input, double expectedInches)
    {
        var len = Length.Parse(input);
        Assert.Equal(expectedInches, len.TotalInches, 8);
    }

    [Fact]
    public void TryParse_ReturnsFalseForUnsupportedInput()
    {
        Assert.False(Length.TryParse("abc", out _));
    }

    [Theory]
    [InlineData("-1 in")]
    [InlineData("-1'")]
    [InlineData("1/-2\"")]
    [InlineData("1/0\"")]
    [InlineData("1 parsec")]
    [InlineData("")]
    [InlineData(" ")]
    public void Parse_InvalidInputs_ThrowFormatException(string input)
    {
        Assert.Throws<FormatException>(() => _ = Length.Parse(input));
    }
}
