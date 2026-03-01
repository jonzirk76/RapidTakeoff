using RapidTakeoff.Core.Projects;
using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Domain;

/// <summary>
/// Native project model used by takeoff calculations.
/// </summary>
public sealed class TakeoffProject
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the common wall height.
    /// </summary>
    public required Length WallHeight { get; init; }

    /// <summary>
    /// Gets wall lengths by wall index.
    /// </summary>
    public required IReadOnlyList<Length> WallLengths { get; init; }

    /// <summary>
    /// Gets takeoff settings.
    /// </summary>
    public required ProjectSettings Settings { get; init; }

    /// <summary>
    /// Gets project penetrations/openings.
    /// </summary>
    public required IReadOnlyList<TakeoffPenetration> Penetrations { get; init; }

    /// <summary>
    /// Gets gross wall area in square feet before penetration deductions.
    /// </summary>
    public double GetGrossWallAreaSquareFeet()
    {
        var totalWallLengthInches = WallLengths.Sum(length => length.TotalInches);
        return Area.FromSquareInches(totalWallLengthInches * WallHeight.TotalInches).TotalSquareFeet;
    }

    /// <summary>
    /// Gets merged penetration area in square feet.
    /// </summary>
    public double GetPenetrationAreaSquareFeet()
    {
        if (Penetrations.Count == 0)
            return 0.0;

        var totalSquareInches = 0.0;
        var byWall = Penetrations.GroupBy(p => p.WallIndex);

        foreach (var wall in byWall)
        {
            var wallRects = wall.ToArray();
            if (wallRects.Length == 0)
                continue;

            var xBreaks = wallRects
                .SelectMany(p => new[] { p.X.TotalInches, p.X.TotalInches + p.Width.TotalInches })
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            for (var i = 0; i < xBreaks.Length - 1; i++)
            {
                var x0 = xBreaks[i];
                var x1 = xBreaks[i + 1];
                var slabWidth = x1 - x0;
                if (slabWidth <= Epsilon)
                    continue;

                var yIntervals = wallRects
                    .Where(p => p.X.TotalInches < x1 - Epsilon && p.X.TotalInches + p.Width.TotalInches > x0 + Epsilon)
                    .Select(p => (Start: p.Y.TotalInches, End: p.Y.TotalInches + p.Height.TotalInches))
                    .OrderBy(t => t.Start)
                    .ToArray();

                if (yIntervals.Length == 0)
                    continue;

                var coveredY = 0.0;
                var currentStart = yIntervals[0].Start;
                var currentEnd = yIntervals[0].End;

                for (var j = 1; j < yIntervals.Length; j++)
                {
                    var interval = yIntervals[j];
                    if (interval.Start <= currentEnd + Epsilon)
                    {
                        currentEnd = Math.Max(currentEnd, interval.End);
                    }
                    else
                    {
                        coveredY += currentEnd - currentStart;
                        currentStart = interval.Start;
                        currentEnd = interval.End;
                    }
                }

                coveredY += currentEnd - currentStart;
                totalSquareInches += slabWidth * coveredY;
            }
        }

        return Area.FromSquareInches(totalSquareInches).TotalSquareFeet;
    }

    /// <summary>
    /// Gets net wall area in square feet after penetration deductions.
    /// </summary>
    public double GetNetWallAreaSquareFeet()
    {
        return Math.Max(0.0, GetGrossWallAreaSquareFeet() - GetPenetrationAreaSquareFeet());
    }

    /// <summary>
    /// Returns non-fatal overlap warnings for penetrations.
    /// </summary>
    public IReadOnlyList<string> GetValidationWarnings()
    {
        var warnings = new List<string>();
        if (Penetrations.Count == 0)
            return warnings;

        foreach (var wallGroup in Penetrations.GroupBy(p => p.WallIndex))
        {
            var items = wallGroup.ToArray();
            for (var i = 0; i < items.Length; i++)
            {
                for (var j = i + 1; j < items.Length; j++)
                {
                    if (RectanglesOverlap(items[i], items[j]))
                    {
                        warnings.Add(
                            $"Penetrations '{Label(items[i], i)}' and '{Label(items[j], j)}' overlap on wall {wallGroup.Key + 1}. " +
                            "Net area uses merged opening area (not double-counted).");
                    }
                }
            }
        }

        return warnings;
    }

    private static bool RectanglesOverlap(TakeoffPenetration a, TakeoffPenetration b)
    {
        if (a.WallIndex != b.WallIndex)
            return false;

        var xOverlap = a.X.TotalInches < (b.X.TotalInches + b.Width.TotalInches) - Epsilon
            && (a.X.TotalInches + a.Width.TotalInches) > b.X.TotalInches + Epsilon;
        var yOverlap = a.Y.TotalInches < (b.Y.TotalInches + b.Height.TotalInches) - Epsilon
            && (a.Y.TotalInches + a.Height.TotalInches) > b.Y.TotalInches + Epsilon;
        return xOverlap && yOverlap;
    }

    private static string Label(TakeoffPenetration penetration, int penetrationIndex)
    {
        return string.IsNullOrWhiteSpace(penetration.Id) ? $"#{penetrationIndex + 1}" : penetration.Id;
    }
}
