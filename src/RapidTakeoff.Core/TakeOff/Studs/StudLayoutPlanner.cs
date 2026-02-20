namespace RapidTakeoff.Core.Takeoff.Studs;

/// <summary>
/// Represents a linear span along wall length in feet.
/// </summary>
/// <param name="StartFeet">Span start in feet.</param>
/// <param name="EndFeet">Span end in feet.</param>
public readonly record struct LinearSpan(double StartFeet, double EndFeet);

/// <summary>
/// Generates and filters rough stud centerline layouts.
/// </summary>
public static class StudLayoutPlanner
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Generates stud centerline X positions in feet for a wall.
    /// Includes both end studs.
    /// </summary>
    /// <param name="wallLengthFeet">Wall length in feet (&gt; 0).</param>
    /// <param name="spacingInches">Stud spacing in inches (&gt; 0).</param>
    public static IReadOnlyList<double> GenerateStudCentersFeet(double wallLengthFeet, double spacingInches)
    {
        if (double.IsNaN(wallLengthFeet) || double.IsInfinity(wallLengthFeet) || wallLengthFeet <= 0)
            throw new ArgumentOutOfRangeException(nameof(wallLengthFeet), "Wall length must be a finite number greater than zero.");

        if (double.IsNaN(spacingInches) || double.IsInfinity(spacingInches) || spacingInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(spacingInches), "Spacing must be a finite number greater than zero.");

        var spacingFeet = spacingInches / 12.0;
        var centers = new List<double> { 0.0 };

        var x = spacingFeet;
        while (x < wallLengthFeet - Epsilon)
        {
            centers.Add(x);
            x += spacingFeet;
        }

        centers.Add(wallLengthFeet);
        return centers;
    }

    /// <summary>
    /// Removes stud centerlines that fall within any supplied span.
    /// Span boundaries are treated as inclusive.
    /// </summary>
    /// <param name="studCentersFeet">Stud centerline X positions in feet.</param>
    /// <param name="removalSpans">Spans where studs should be removed.</param>
    public static IReadOnlyList<double> RemoveCentersInsideSpans(
        IReadOnlyList<double> studCentersFeet,
        IReadOnlyList<LinearSpan> removalSpans)
    {
        ArgumentNullException.ThrowIfNull(studCentersFeet);
        ArgumentNullException.ThrowIfNull(removalSpans);

        if (removalSpans.Count == 0)
            return studCentersFeet.ToArray();

        var normalizedSpans = removalSpans
            .Select(span => span.StartFeet <= span.EndFeet ? span : new LinearSpan(span.EndFeet, span.StartFeet))
            .ToArray();

        return studCentersFeet
            .Where(center => !normalizedSpans.Any(span => center >= span.StartFeet - Epsilon && center <= span.EndFeet + Epsilon))
            .ToArray();
    }

    /// <summary>
    /// Adds king-stud centerlines at both sides of each opening span.
    /// Side centerlines are offset by 1.5x stud width from opening edges
    /// (trimmer at 0.5x, king attached outside trimmer by another stud width).
    /// </summary>
    /// <param name="studCentersFeet">Current stud centerline X positions in feet.</param>
    /// <param name="openingSpans">Opening spans in wall-local X coordinates.</param>
    /// <param name="studWidthFeet">Stud width in feet (&gt; 0).</param>
    /// <param name="wallLengthFeet">Wall length in feet (&gt; 0).</param>
    public static IReadOnlyList<double> AddKingStudCenters(
        IReadOnlyList<double> studCentersFeet,
        IReadOnlyList<LinearSpan> openingSpans,
        double studWidthFeet,
        double wallLengthFeet)
    {
        ArgumentNullException.ThrowIfNull(studCentersFeet);
        ArgumentNullException.ThrowIfNull(openingSpans);

        if (double.IsNaN(studWidthFeet) || double.IsInfinity(studWidthFeet) || studWidthFeet <= 0)
            throw new ArgumentOutOfRangeException(nameof(studWidthFeet), "Stud width must be a finite number greater than zero.");

        if (double.IsNaN(wallLengthFeet) || double.IsInfinity(wallLengthFeet) || wallLengthFeet <= 0)
            throw new ArgumentOutOfRangeException(nameof(wallLengthFeet), "Wall length must be a finite number greater than zero.");

        if (openingSpans.Count == 0)
            return studCentersFeet.ToArray();

        var kingOffset = studWidthFeet * 1.5;
        var allCenters = new List<double>(studCentersFeet);

        foreach (var rawSpan in openingSpans)
        {
            var span = rawSpan.StartFeet <= rawSpan.EndFeet
                ? rawSpan
                : new LinearSpan(rawSpan.EndFeet, rawSpan.StartFeet);

            var leftKing = span.StartFeet - kingOffset;
            var rightKing = span.EndFeet + kingOffset;

            if (leftKing >= 0.0 - Epsilon && leftKing <= wallLengthFeet + Epsilon)
                allCenters.Add(Math.Clamp(leftKing, 0.0, wallLengthFeet));

            if (rightKing >= 0.0 - Epsilon && rightKing <= wallLengthFeet + Epsilon)
                allCenters.Add(Math.Clamp(rightKing, 0.0, wallLengthFeet));
        }

        return allCenters
            .OrderBy(x => x)
            .Aggregate(
                seed: new List<double>(),
                (acc, x) =>
                {
                    if (acc.Count == 0 || Math.Abs(acc[^1] - x) > Epsilon)
                        acc.Add(x);
                    return acc;
                });
    }
}
