namespace RapidTakeoff.Core.Units;

/// <summary>
/// Represents a non-negative length measurement.
/// Internally stored in inches to align with common residential takeoff inputs
/// (stud spacing, sheet dimensions, etc.).
/// </summary>
public readonly record struct Length
{
    /// <summary>
    /// Gets the total length in inches.
    /// </summary>
    public double TotalInches { get; }

    /// <summary>
    /// Gets the total length in feet.
    /// </summary>
    public double TotalFeet => TotalInches / 12.0;

    private Length(double totalInches)
    {
        if (double.IsNaN(totalInches) || double.IsInfinity(totalInches))
            throw new ArgumentOutOfRangeException(nameof(totalInches), "Length must be a finite number.");

        if (totalInches < 0)
            throw new ArgumentOutOfRangeException(nameof(totalInches), "Length cannot be negative.");

        TotalInches = totalInches;
    }

    /// <summary>
    /// Creates a <see cref="Length"/> from inches.
    /// </summary>
    /// <param name="inches">Total inches (must be finite and non-negative).</param>
    public static Length FromInches(double inches) => new(inches);

    /// <summary>
    /// Creates a <see cref="Length"/> from feet.
    /// </summary>
    /// <param name="feet">Total feet (must be finite and non-negative).</param>
    public static Length FromFeet(double feet) => new(feet * 12.0);

    /// <summary>
    /// Creates a <see cref="Length"/> from a feet + inches pair.
    /// </summary>
    /// <param name="feet">Feet portion (must be finite and non-negative).</param>
    /// <param name="inches">Inches portion (must be finite and non-negative).</param>
    public static Length FromFeetAndInches(double feet, double inches)
    {
        if (double.IsNaN(feet) || double.IsInfinity(feet))
            throw new ArgumentOutOfRangeException(nameof(feet), "Feet must be a finite number.");
        if (double.IsNaN(inches) || double.IsInfinity(inches))
            throw new ArgumentOutOfRangeException(nameof(inches), "Inches must be a finite number.");
        if (feet < 0)
            throw new ArgumentOutOfRangeException(nameof(feet), "Feet cannot be negative.");
        if (inches < 0)
            throw new ArgumentOutOfRangeException(nameof(inches), "Inches cannot be negative.");

        return new Length((feet * 12.0) + inches);
    }

    /// <summary>
    /// Adds two lengths.
    /// </summary>
    public static Length operator +(Length a, Length b) => new(a.TotalInches + b.TotalInches);

    /// <summary>
    /// Subtracts one length from another. Result must be non-negative.
    /// </summary>
    public static Length operator -(Length a, Length b) => new(a.TotalInches - b.TotalInches);

    /// <summary>
    /// Scales a length by a factor. Result must be non-negative.
    /// </summary>
    public static Length operator *(Length a, double factor) => new(a.TotalInches * factor);

    /// <summary>
    /// Scales a length by a factor. Result must be non-negative.
    /// </summary>
    public static Length operator *(double factor, Length a) => a * factor;

    /// <summary>
    /// Divides a length by a divisor. Result must be non-negative.
    /// </summary>
    public static Length operator /(Length a, double divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Divisor cannot be zero.");
        return new Length(a.TotalInches / divisor);
    }

    /// <summary>
    /// Returns a human-friendly representation in feet and inches.
    /// </summary>
    public override string ToString()
    {
        // Keep it simple and non-rounding-opinionated for now.
        return $"{TotalFeet:0.###} ft ({TotalInches:0.###} in)";
    }
}
