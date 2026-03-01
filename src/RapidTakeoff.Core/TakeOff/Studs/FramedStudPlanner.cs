using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Studs;

/// <summary>
/// Opening geometry used by framed stud planning.
/// Coordinates are in wall-local feet.
/// </summary>
/// <param name="X">Opening left edge from wall start.</param>
/// <param name="Y">Opening bottom from floor.</param>
/// <param name="Width">Opening width.</param>
/// <param name="Height">Opening height.</param>
public readonly record struct StudOpening(
    Length X,
    Length Y,
    Length Width,
    Length Height);

/// <summary>
/// Framed stud plan for a single wall.
/// </summary>
public sealed class FramedWallStudPlan
{
    /// <summary>
    /// Gets nominal stud centers from the initial spacing pass.
    /// </summary>
    public IReadOnlyList<Length> NominalCenters { get; }

    /// <summary>
    /// Gets common stud centers after opening-zone removal.
    /// </summary>
    public IReadOnlyList<Length> CommonCenters { get; }

    /// <summary>
    /// Gets king stud centers.
    /// </summary>
    public IReadOnlyList<Length> KingCenters { get; }

    /// <summary>
    /// Gets final rendered/quantified stud centers (common + kings, deduplicated).
    /// </summary>
    public IReadOnlyList<Length> FinalCenters { get; }

    /// <summary>
    /// Gets trimmer stud count.
    /// </summary>
    public int TrimmerCount { get; }

    /// <summary>
    /// Gets top cripple stud count.
    /// </summary>
    public int CrippleTopCount { get; }

    /// <summary>
    /// Gets bottom cripple stud count.
    /// </summary>
    public int CrippleBottomCount { get; }

    /// <summary>
    /// Gets base stud count for this wall.
    /// </summary>
    public int BaseStudCount => FinalCenters.Count + TrimmerCount + CrippleTopCount + CrippleBottomCount;

    /// <summary>
    /// Initializes a new framed wall stud plan.
    /// </summary>
    public FramedWallStudPlan(
        IReadOnlyList<Length> nominalCenters,
        IReadOnlyList<Length> commonCenters,
        IReadOnlyList<Length> kingCenters,
        IReadOnlyList<Length> finalCenters,
        int trimmerCount,
        int crippleTopCount,
        int crippleBottomCount)
    {
        NominalCenters = nominalCenters;
        CommonCenters = commonCenters;
        KingCenters = kingCenters;
        FinalCenters = finalCenters;
        TrimmerCount = trimmerCount;
        CrippleTopCount = crippleTopCount;
        CrippleBottomCount = crippleBottomCount;
    }
}

/// <summary>
/// Builds framed stud plans (common studs, opening framing, and cripples).
/// </summary>
public static class FramedStudPlanner
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Builds a framed stud plan for one wall.
    /// </summary>
    public static FramedWallStudPlan BuildWallPlan(
        Length wallLength,
        Length wallHeight,
        Length spacing,
        Length studWidth,
        IReadOnlyList<StudOpening> openings)
    {
        var wallLengthInches = wallLength.TotalInches;
        var wallHeightInches = wallHeight.TotalInches;
        var studWidthInches = studWidth.TotalInches;

        var nominalCenters = StudLayoutPlanner.GenerateStudCenters(wallLength, spacing);
        if (openings.Count == 0)
        {
            return new FramedWallStudPlan(
                nominalCenters,
                nominalCenters,
                [],
                nominalCenters,
                trimmerCount: 0,
                crippleTopCount: 0,
                crippleBottomCount: 0);
        }

        var halfStudInches = studWidthInches / 2.0;
        var framingOffsetInches = studWidthInches * 1.5;

        var openingSpans = openings
            .Select(p => new LinearSpan(p.X, p.X + p.Width))
            .ToArray();

        var framedOpeningSpans = openings
            .Select(p => new LinearSpan(
                Length.FromInches(Math.Max(0.0, p.X.TotalInches - framingOffsetInches)),
                Length.FromInches(Math.Min(wallLengthInches, p.X.TotalInches + p.Width.TotalInches + framingOffsetInches))))
            .ToArray();

        var commonCenters = StudLayoutPlanner.RemoveCentersInsideSpans(nominalCenters, framedOpeningSpans);
        var kingCenters = StudLayoutPlanner.AddKingStudCenters(
            Array.Empty<Length>(),
            openingSpans,
            studWidth,
            wallLength);

        var trimmerCount = 0;
        var crippleTopCount = 0;
        var crippleBottomCount = 0;

        foreach (var opening in openings)
        {
            var openingLeft = opening.X.TotalInches;
            var openingRight = opening.X.TotalInches + opening.Width.TotalInches;
            var openingBottom = opening.Y.TotalInches;
            var openingTop = opening.Y.TotalInches + opening.Height.TotalInches;

            var leftTrimmer = openingLeft - halfStudInches;
            var rightTrimmer = openingRight + halfStudInches;
            if (leftTrimmer >= -Epsilon && leftTrimmer <= wallLengthInches + Epsilon) trimmerCount++;
            if (rightTrimmer >= -Epsilon && rightTrimmer <= wallLengthInches + Epsilon) trimmerCount++;

            var interiorCount = nominalCenters.Count(center => center.TotalInches > openingLeft + Epsilon && center.TotalInches < openingRight - Epsilon);

            var headerY = openingTop + halfStudInches;
            if (headerY < wallHeightInches - Epsilon)
                crippleTopCount += interiorCount;

            var hasSill = openingBottom > Epsilon;
            var sillY = openingBottom - halfStudInches;
            if (hasSill && sillY > Epsilon)
                crippleBottomCount += interiorCount;
        }

        var finalCenters = MergeDistinctCenters(commonCenters, kingCenters);
        return new FramedWallStudPlan(
            nominalCenters,
            commonCenters,
            kingCenters,
            finalCenters,
            trimmerCount,
            crippleTopCount,
            crippleBottomCount);
    }

    private static IReadOnlyList<Length> MergeDistinctCenters(IReadOnlyList<Length> a, IReadOnlyList<Length> b)
    {
        return a.Concat(b)
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
