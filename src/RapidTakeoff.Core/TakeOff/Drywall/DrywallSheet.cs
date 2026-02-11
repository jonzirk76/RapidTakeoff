using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Drywall;

/// <summary>
/// Represents a rectangular sheet good (e.g., drywall or plywood) used for takeoff.
/// </summary>
public readonly record struct DrywallSheet
{
    /// <summary>
    /// Gets the sheet width.
    /// </summary>
    public Length Width { get; }

    /// <summary>
    /// Gets the sheet height/length.
    /// </summary>
    public Length Height { get; }

    /// <summary>
    /// Gets the sheet area.
    /// </summary>
    public Area Area => Area.FromRectangle(Width, Height);

    private DrywallSheet(Length width, Length height)
    {
        if (width.TotalInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        if (height.TotalInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a sheet from width and height.
    /// </summary>
    public static DrywallSheet FromDimensions(Length width, Length height) => new(width, height);

    /// <summary>
    /// Standard 4x8 sheet.
    /// </summary>
    public static DrywallSheet Sheet4x8 => new(Length.FromFeet(4), Length.FromFeet(8));

    /// <summary>
    /// Standard 4x12 sheet.
    /// </summary>
    public static DrywallSheet Sheet4x12 => new(Length.FromFeet(4), Length.FromFeet(12));
}
