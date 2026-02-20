namespace RapidTakeoff.Core.Takeoff.Studs;

/// <summary>
/// Opening geometry used by framed stud planning.
/// Coordinates are in wall-local feet.
/// </summary>
/// <param name="XFeet">Opening left edge from wall start.</param>
/// <param name="YFeet">Opening bottom from floor.</param>
/// <param name="WidthFeet">Opening width in feet.</param>
/// <param name="HeightFeet">Opening height in feet.</param>
public readonly record struct StudOpening(
    double XFeet,
    double YFeet,
    double WidthFeet,
    double HeightFeet);

/// <summary>
/// Framed stud plan for a single wall.
/// </summary>
public sealed class FramedWallStudPlan
{
    /// <summary>
    /// Gets nominal stud centers from the initial spacing pass.
    /// </summary>
    public IReadOnlyList<double> NominalCenters { get; }

    /// <summary>
    /// Gets common stud centers after opening-zone removal.
    /// </summary>
    public IReadOnlyList<double> CommonCenters { get; }

    /// <summary>
    /// Gets king stud centers.
    /// </summary>
    public IReadOnlyList<double> KingCenters { get; }

    /// <summary>
    /// Gets final rendered/quantified stud centers (common + kings, deduplicated).
    /// </summary>
    public IReadOnlyList<double> FinalCenters { get; }

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
        IReadOnlyList<double> nominalCenters,
        IReadOnlyList<double> commonCenters,
        IReadOnlyList<double> kingCenters,
        IReadOnlyList<double> finalCenters,
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
        double wallLengthFeet,
        double wallHeightFeet,
        double spacingInches,
        double studWidthFeet,
        IReadOnlyList<StudOpening> openings)
    {
        var nominalCenters = StudLayoutPlanner.GenerateStudCentersFeet(wallLengthFeet, spacingInches);
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

        var halfStud = studWidthFeet / 2.0;
        var framingOffsetFeet = studWidthFeet * 1.5;

        var openingSpans = openings
            .Select(p => new LinearSpan(p.XFeet, p.XFeet + p.WidthFeet))
            .ToArray();

        var framedOpeningSpans = openings
            .Select(p => new LinearSpan(p.XFeet - framingOffsetFeet, p.XFeet + p.WidthFeet + framingOffsetFeet))
            .ToArray();

        var commonCenters = StudLayoutPlanner.RemoveCentersInsideSpans(nominalCenters, framedOpeningSpans);
        var kingCenters = StudLayoutPlanner.AddKingStudCenters(
            Array.Empty<double>(),
            openingSpans,
            studWidthFeet,
            wallLengthFeet);

        var trimmerCount = 0;
        var crippleTopCount = 0;
        var crippleBottomCount = 0;

        foreach (var opening in openings)
        {
            var openingLeft = opening.XFeet;
            var openingRight = opening.XFeet + opening.WidthFeet;
            var openingBottom = opening.YFeet;
            var openingTop = opening.YFeet + opening.HeightFeet;

            var leftTrimmer = openingLeft - halfStud;
            var rightTrimmer = openingRight + halfStud;
            if (leftTrimmer >= -Epsilon && leftTrimmer <= wallLengthFeet + Epsilon) trimmerCount++;
            if (rightTrimmer >= -Epsilon && rightTrimmer <= wallLengthFeet + Epsilon) trimmerCount++;

            var interiorCount = nominalCenters.Count(center => center > openingLeft + Epsilon && center < openingRight - Epsilon);

            var headerY = openingTop + halfStud;
            if (headerY < wallHeightFeet - Epsilon)
                crippleTopCount += interiorCount;

            var hasSill = openingBottom > Epsilon;
            var sillY = openingBottom - halfStud;
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

    private static IReadOnlyList<double> MergeDistinctCenters(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        return a.Concat(b)
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
