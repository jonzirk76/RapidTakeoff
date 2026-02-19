namespace RapidTakeoff.Core.Projects;

/// <summary>
/// Represents a simple project definition used for takeoff calculations.
/// </summary>
public sealed class Project
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets wall height in feet.
    /// </summary>
    public double WallHeightFeet { get; set; }

    /// <summary>
    /// Gets or sets wall lengths in feet.
    /// </summary>
    public double[] WallLengthsFeet { get; set; } = [];

    /// <summary>
    /// Gets or sets project-level takeoff settings.
    /// </summary>
    public ProjectSettings Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets wall penetrations/openings used to compute net wall area.
    /// </summary>
    public ProjectPenetration[] Penetrations { get; set; } = [];

    /// <summary>
    /// Validates project values and throws when invalid.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentOutOfRangeException(nameof(Name), "Project name is required.");

        ValidateFiniteNonNegative(WallHeightFeet, nameof(WallHeightFeet));

        if (WallLengthsFeet is null)
            throw new ArgumentNullException(nameof(WallLengthsFeet), "Wall lengths are required.");

        if (WallLengthsFeet.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(WallLengthsFeet), "At least one wall length is required.");

        for (var i = 0; i < WallLengthsFeet.Length; i++)
        {
            ValidateFiniteNonNegative(WallLengthsFeet[i], nameof(WallLengthsFeet));
        }

        if (Penetrations is null)
            throw new ArgumentNullException(nameof(Penetrations), "Penetrations collection cannot be null.");

        for (var i = 0; i < Penetrations.Length; i++)
        {
            var penetration = Penetrations[i]
                ?? throw new ArgumentOutOfRangeException(nameof(Penetrations), $"Penetration at index {i} is null.");

            ValidatePenetration(penetration, i);
        }

        ArgumentNullException.ThrowIfNull(Settings);
        Settings.Validate();
    }

    /// <summary>
    /// Gets the gross wall area in square feet before penetration deductions.
    /// </summary>
    public double GetGrossWallAreaSquareFeet()
    {
        return WallLengthsFeet.Sum() * WallHeightFeet;
    }

    /// <summary>
    /// Gets the total penetration/opening area in square feet, merged per wall so overlaps are not double-counted.
    /// </summary>
    public double GetPenetrationAreaSquareFeet()
    {
        if (Penetrations is null || Penetrations.Length == 0)
            return 0.0;

        var total = 0.0;
        var byWall = Penetrations.GroupBy(p => p.WallIndex);

        foreach (var wall in byWall)
        {
            var wallRects = wall.ToArray();
            if (wallRects.Length == 0)
                continue;

            var xBreaks = wallRects
                .SelectMany(p => new[] { p.XFeet, p.XFeet + p.WidthFeet })
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
                    .Where(p => p.XFeet < x1 - Epsilon && p.XFeet + p.WidthFeet > x0 + Epsilon)
                    .Select(p => (Start: p.YFeet, End: p.YFeet + p.HeightFeet))
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
                total += slabWidth * coveredY;
            }
        }

        return total;
    }

    /// <summary>
    /// Gets the net wall area in square feet after penetration deductions.
    /// </summary>
    public double GetNetWallAreaSquareFeet()
    {
        return Math.Max(0.0, GetGrossWallAreaSquareFeet() - GetPenetrationAreaSquareFeet());
    }

    /// <summary>
    /// Returns friendly, non-fatal validation warnings.
    /// </summary>
    public IReadOnlyList<string> GetValidationWarnings()
    {
        var warnings = new List<string>();
        if (Penetrations is null || Penetrations.Length == 0)
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

    private static void ValidateFiniteNonNegative(double value, string paramName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be a finite number.");
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} cannot be negative.");
    }

    private static void ValidateFinitePositive(double value, string paramName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be a finite number.");
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be greater than zero.");
    }

    private void ValidatePenetration(ProjectPenetration penetration, int penetrationIndex)
    {
        var label = Label(penetration, penetrationIndex);

        if (string.IsNullOrWhiteSpace(penetration.Type))
            throw new ArgumentOutOfRangeException(nameof(Penetrations), $"Penetration '{label}' must include a type.");

        if (penetration.WallIndex < 0 || penetration.WallIndex >= WallLengthsFeet.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Penetrations),
                $"Penetration '{label}' references wall index {penetration.WallIndex}, but valid range is 0..{WallLengthsFeet.Length - 1}.");
        }

        ValidateFiniteNonNegative(penetration.XFeet, nameof(Penetrations));
        ValidateFiniteNonNegative(penetration.YFeet, nameof(Penetrations));
        ValidateFinitePositive(penetration.WidthFeet, nameof(Penetrations));
        ValidateFinitePositive(penetration.HeightFeet, nameof(Penetrations));

        var wallLength = WallLengthsFeet[penetration.WallIndex];
        var rightEdge = penetration.XFeet + penetration.WidthFeet;
        if (rightEdge > wallLength + Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Penetrations),
                $"Penetration '{label}' exceeds wall {penetration.WallIndex + 1} length: x+width={rightEdge:0.###} ft > {wallLength:0.###} ft.");
        }

        var topEdge = penetration.YFeet + penetration.HeightFeet;
        if (topEdge > WallHeightFeet + Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Penetrations),
                $"Penetration '{label}' exceeds wall height: y+height={topEdge:0.###} ft > {WallHeightFeet:0.###} ft.");
        }
    }

    private static bool RectanglesOverlap(ProjectPenetration a, ProjectPenetration b)
    {
        if (a.WallIndex != b.WallIndex)
            return false;

        var xOverlap = a.XFeet < (b.XFeet + b.WidthFeet) - Epsilon && (a.XFeet + a.WidthFeet) > b.XFeet + Epsilon;
        var yOverlap = a.YFeet < (b.YFeet + b.HeightFeet) - Epsilon && (a.YFeet + a.HeightFeet) > b.YFeet + Epsilon;
        return xOverlap && yOverlap;
    }

    private static string Label(ProjectPenetration penetration, int penetrationIndex)
    {
        return string.IsNullOrWhiteSpace(penetration.Id) ? $"#{penetrationIndex + 1}" : penetration.Id;
    }
}
