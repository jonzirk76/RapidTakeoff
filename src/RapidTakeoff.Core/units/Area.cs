namespace RapidTakeoff.Core.Units;

/// <summary>
/// Represents a non-negative area measurement.
/// Internally stored in square inches to align with imperial takeoff inputs,
/// while still allowing easy conversion to square feet.
/// </summary>
public readonly record struct Area
{
    /// <summary>
    /// Gets the total area in square inches.
    /// </summary>
    public double TotalSquareInches { get; }

    /// <summary>
    /// Gets the total area in square feet.
    /// </summary>
    public double TotalSquareFeet => TotalSquareInches / 144.0; // 12*12

    private Area(double totalSquareInches)
    {
        if (double.IsNaN(totalSquareInches) || double.IsInfinity(totalSquareInches))
            throw new ArgumentOutOfRangeException(nameof(totalSquareInches), "Area must be a finite number.");

        if (totalSquareInches < 0)
            throw new ArgumentOutOfRangeException(nameof(totalSquareInches), "Area cannot be negative.");

        TotalSquareInches = totalSquareInches;
    }

    /// <summary>
    /// Creates an <see cref="Area"/> from square inches.
    /// </summary>
    /// <param name="squareInches">Square inches (must be finite and non-negative).</param>
    public static Area FromSquareInches(double squareInches) => new(squareInches);

    /// <summary>
    /// Creates an <see cref="Area"/> from square feet.
    /// </summary>
    /// <param name="squareFeet">Square feet (must be finite and non-negative).</param>
    public static Area FromSquareFeet(double squareFeet) => new(squareFeet * 144.0);

    /// <summary>
    /// Adds two areas.
    /// </summary>
    public static Area operator +(Area a, Area b) => new(a.TotalSquareInches + b.TotalSquareInches);

    /// <summary>
    /// Subtracts one area from another. Result must be non-negative.
    /// </summary>
    public static Area operator -(Area a, Area b) => new(a.TotalSquareInches - b.TotalSquareInches);

    /// <summary>
    /// Scales an area by a factor. Result must be non-negative.
    /// </summary>
    public static Area operator *(Area a, double factor) => new(a.TotalSquareInches * factor);

    /// <summary>
    /// Scales an area by a factor. Result must be non-negative.
    /// </summary>
    public static Area operator *(double factor, Area a) => a * factor;

    /// <summary>
    /// Divides an area by a divisor. Result must be non-negative.
    /// </summary>
    public static Area operator /(Area a, double divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Divisor cannot be zero.");
        return new Area(a.TotalSquareInches / divisor);
    }

    /// <summary>
    /// Returns a human-friendly representation in square feet and square inches.
    /// </summary>
    public override string ToString()
    {
        return $"{TotalSquareFeet:0.###} sqft ({TotalSquareInches:0.###} sqin)";
    }
}
