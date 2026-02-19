namespace RapidTakeoff.Rendering.Walls;

/// <summary>
/// Render-ready stud layout for a wall elevation.
/// </summary>
/// <param name="StudType">Nominal stud type used to derive displayed stud width.</param>
/// <param name="SpacingInches">Requested framing spacing in inches.</param>
/// <param name="StudCenterXFeet">Stud centerline positions in wall-local X coordinates (feet).</param>
public sealed record StudLayoutDto(
    StudTypeDto StudType,
    double SpacingInches,
    IReadOnlyList<double> StudCenterXFeet
);

/// <summary>
/// Provides displayed stud widths for each nominal stud type.
/// </summary>
public static class StudTypeDisplay
{
    /// <summary>
    /// Gets displayed stud width in feet for the given nominal stud type.
    /// </summary>
    public static double GetWidthFeet(StudTypeDto studType)
    {
        // Typical dressed stud depths (inches) converted to feet.
        return studType switch
        {
            StudTypeDto.TwoByFour => 3.5 / 12.0,
            StudTypeDto.TwoBySix => 5.5 / 12.0,
            StudTypeDto.TwoByEight => 7.25 / 12.0,
            StudTypeDto.TwoByTen => 9.25 / 12.0,
            StudTypeDto.TwoByTwelve => 11.25 / 12.0,
            _ => 3.5 / 12.0
        };
    }
}
