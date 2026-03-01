using RapidTakeoff.Core.Domain;
using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Projects;

/// <summary>
/// Normalizes JSON-backed project DTOs to native domain models and validates inputs.
/// </summary>
public static class ProjectNormalizer
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Validates and converts a JSON project DTO to a normalized native project model.
    /// </summary>
    public static TakeoffProject Normalize(Project raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        if (string.IsNullOrWhiteSpace(raw.Name))
            throw new ArgumentOutOfRangeException(nameof(raw.Name), "Project name is required.");

        ValidateFiniteNonNegative(raw.WallHeightFeet, nameof(raw.WallHeightFeet));

        if (raw.WallLengthsFeet is null)
            throw new ArgumentNullException(nameof(raw.WallLengthsFeet), "Wall lengths are required.");

        if (raw.WallLengthsFeet.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(raw.WallLengthsFeet), "At least one wall length is required.");

        var wallLengths = new Length[raw.WallLengthsFeet.Length];
        for (var i = 0; i < raw.WallLengthsFeet.Length; i++)
        {
            var wallLengthFeet = raw.WallLengthsFeet[i];
            ValidateFiniteNonNegative(wallLengthFeet, nameof(raw.WallLengthsFeet));
            wallLengths[i] = Length.FromFeet(wallLengthFeet);
        }

        if (raw.Penetrations is null)
            throw new ArgumentNullException(nameof(raw.Penetrations), "Penetrations collection cannot be null.");

        ArgumentNullException.ThrowIfNull(raw.Settings);
        raw.Settings.Validate();

        var wallHeight = Length.FromFeet(raw.WallHeightFeet);
        var penetrations = new List<TakeoffPenetration>(raw.Penetrations.Length);
        for (var i = 0; i < raw.Penetrations.Length; i++)
        {
            var penetration = raw.Penetrations[i]
                ?? throw new ArgumentOutOfRangeException(nameof(raw.Penetrations), $"Penetration at index {i} is null.");

            var normalized = NormalizePenetration(penetration, i, wallLengths, wallHeight);
            penetrations.Add(normalized);
        }

        return new TakeoffProject
        {
            Name = raw.Name.Trim(),
            WallHeight = wallHeight,
            WallLengths = wallLengths,
            Settings = raw.Settings,
            Penetrations = penetrations
        };
    }

    private static TakeoffPenetration NormalizePenetration(
        ProjectPenetration penetration,
        int penetrationIndex,
        IReadOnlyList<Length> wallLengths,
        Length wallHeight)
    {
        var label = Label(penetration, penetrationIndex);

        if (string.IsNullOrWhiteSpace(penetration.Type))
            throw new ArgumentOutOfRangeException(nameof(Project.Penetrations), $"Penetration '{label}' must include a type.");

        if (penetration.WallIndex < 0 || penetration.WallIndex >= wallLengths.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Project.Penetrations),
                $"Penetration '{label}' references wall index {penetration.WallIndex}, but valid range is 0..{wallLengths.Count - 1}.");
        }

        ValidateFiniteNonNegative(penetration.XFeet, nameof(Project.Penetrations));
        ValidateFiniteNonNegative(penetration.YFeet, nameof(Project.Penetrations));
        ValidateFinitePositive(penetration.WidthFeet, nameof(Project.Penetrations));
        ValidateFinitePositive(penetration.HeightFeet, nameof(Project.Penetrations));

        var x = Length.FromFeet(penetration.XFeet);
        var y = Length.FromFeet(penetration.YFeet);
        var width = Length.FromFeet(penetration.WidthFeet);
        var height = Length.FromFeet(penetration.HeightFeet);
        var wallLength = wallLengths[penetration.WallIndex];

        if (x.TotalInches > wallLength.TotalInches + Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Project.Penetrations),
                $"Penetration '{label}' exceeds wall {penetration.WallIndex + 1} length: x={x.TotalFeet:0.###} ft > {wallLength.TotalFeet:0.###} ft.");
        }

        if (y.TotalInches > wallHeight.TotalInches + Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Project.Penetrations),
                $"Penetration '{label}' exceeds wall height: y={y.TotalFeet:0.###} ft > {wallHeight.TotalFeet:0.###} ft.");
        }

        var rightEdge = x + width;
        if (rightEdge.TotalInches > wallLength.TotalInches + Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Project.Penetrations),
                $"Penetration '{label}' exceeds wall {penetration.WallIndex + 1} length: x+width={rightEdge.TotalFeet:0.###} ft > {wallLength.TotalFeet:0.###} ft.");
        }

        var topEdge = y + height;
        if (topEdge.TotalInches > wallHeight.TotalInches + Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Project.Penetrations),
                $"Penetration '{label}' exceeds wall height: y+height={topEdge.TotalFeet:0.###} ft > {wallHeight.TotalFeet:0.###} ft.");
        }

        return new TakeoffPenetration
        {
            Id = penetration.Id ?? string.Empty,
            Type = penetration.Type.Trim(),
            WallIndex = penetration.WallIndex,
            X = x,
            Y = y,
            Width = width,
            Height = height
        };
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

    private static string Label(ProjectPenetration penetration, int penetrationIndex)
    {
        return string.IsNullOrWhiteSpace(penetration.Id) ? $"#{penetrationIndex + 1}" : penetration.Id;
    }
}
