using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Studs;

/// <summary>
/// Represents a linear span along wall length.
/// </summary>
/// <param name="Start">Span start.</param>
/// <param name="End">Span end.</param>
public readonly record struct LinearSpan(Length Start, Length End);

/// <summary>
/// Generates and filters rough stud centerline layouts.
/// </summary>
public static class StudLayoutPlanner
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Generates stud centerline X positions for a wall.
    /// Includes both end studs.
    /// </summary>
    /// <param name="wallLength">Wall length (&gt; 0).</param>
    /// <param name="spacing">Stud spacing (&gt; 0).</param>
    public static IReadOnlyList<Length> GenerateStudCenters(Length wallLength, Length spacing)
    {
        if (wallLength.TotalInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(wallLength), "Wall length must be greater than zero.");

        if (spacing.TotalInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(spacing), "Spacing must be greater than zero.");

        var centers = new List<Length> { Length.FromInches(0.0) };
        var wallLengthInches = wallLength.TotalInches;
        var spacingInches = spacing.TotalInches;

        var x = spacingInches;
        while (x < wallLengthInches - Epsilon)
        {
            centers.Add(Length.FromInches(x));
            x += spacingInches;
        }

        centers.Add(wallLength);
        return centers;
    }

    /// <summary>
    /// Removes stud centerlines that fall within any supplied span.
    /// Span boundaries are treated as inclusive.
    /// </summary>
    /// <param name="studCenters">Stud centerline X positions.</param>
    /// <param name="removalSpans">Spans where studs should be removed.</param>
    public static IReadOnlyList<Length> RemoveCentersInsideSpans(
        IReadOnlyList<Length> studCenters,
        IReadOnlyList<LinearSpan> removalSpans)
    {
        ArgumentNullException.ThrowIfNull(studCenters);
        ArgumentNullException.ThrowIfNull(removalSpans);

        if (removalSpans.Count == 0)
            return studCenters.ToArray();

        var normalizedSpans = removalSpans
            .Select(span => span.Start.TotalInches <= span.End.TotalInches ? span : new LinearSpan(span.End, span.Start))
            .ToArray();

        return studCenters
            .Where(center => !normalizedSpans.Any(span => center.TotalInches >= span.Start.TotalInches - Epsilon && center.TotalInches <= span.End.TotalInches + Epsilon))
            .ToArray();
    }

    /// <summary>
    /// Adds king-stud centerlines at both sides of each opening span.
    /// Side centerlines are offset by 1.5x stud width from opening edges
    /// (trimmer at 0.5x, king attached outside trimmer by another stud width).
    /// </summary>
    /// <param name="studCenters">Current stud centerline X positions.</param>
    /// <param name="openingSpans">Opening spans in wall-local X coordinates.</param>
    /// <param name="studWidth">Stud width (&gt; 0).</param>
    /// <param name="wallLength">Wall length (&gt; 0).</param>
    public static IReadOnlyList<Length> AddKingStudCenters(
        IReadOnlyList<Length> studCenters,
        IReadOnlyList<LinearSpan> openingSpans,
        Length studWidth,
        Length wallLength)
    {
        ArgumentNullException.ThrowIfNull(studCenters);
        ArgumentNullException.ThrowIfNull(openingSpans);

        if (studWidth.TotalInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(studWidth), "Stud width must be greater than zero.");

        if (wallLength.TotalInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(wallLength), "Wall length must be greater than zero.");

        if (openingSpans.Count == 0)
            return studCenters.ToArray();

        var kingOffsetInches = studWidth.TotalInches * 1.5;
        var wallLengthInches = wallLength.TotalInches;
        var allCenters = new List<Length>(studCenters);

        foreach (var rawSpan in openingSpans)
        {
            var span = rawSpan.Start.TotalInches <= rawSpan.End.TotalInches
                ? rawSpan
                : new LinearSpan(rawSpan.End, rawSpan.Start);

            var leftKingInches = span.Start.TotalInches - kingOffsetInches;
            var rightKingInches = span.End.TotalInches + kingOffsetInches;

            if (leftKingInches >= 0.0 - Epsilon && leftKingInches <= wallLengthInches + Epsilon)
                allCenters.Add(Length.FromInches(Math.Clamp(leftKingInches, 0.0, wallLengthInches)));

            if (rightKingInches >= 0.0 - Epsilon && rightKingInches <= wallLengthInches + Epsilon)
                allCenters.Add(Length.FromInches(Math.Clamp(rightKingInches, 0.0, wallLengthInches)));
        }

        return allCenters
            .OrderBy(x => x.TotalInches)
            .Aggregate(
                seed: new List<Length>(),
                (acc, x) =>
                {
                    if (acc.Count == 0 || Math.Abs(acc[^1].TotalInches - x.TotalInches) > Epsilon)
                        acc.Add(x);
                    return acc;
                });
    }
}
