using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Studs;

/// <summary>
/// Calculates stud quantities from wall lengths and on-center spacing.
/// </summary>
public static class StudTakeoffCalculator
{
    /// <summary>
    /// Calculates studs required for multiple walls.
    /// </summary>
    /// <param name="wallLengths">Wall lengths (must be > 0).</param>
    /// <param name="spacingInches">Stud spacing in inches (must be > 0).</param>
    /// <param name="wasteFactor">Waste factor as a fraction (e.g., 0.10 for 10%). Must be >= 0.</param>
    public static StudTakeoffResult Calculate(IReadOnlyList<Length> wallLengths, Length spacingInches, double wasteFactor = 0.0)
    {
        if (wallLengths is null)
            throw new ArgumentNullException(nameof(wallLengths));

        if (double.IsNaN(wasteFactor) || double.IsInfinity(wasteFactor))
            throw new ArgumentOutOfRangeException(nameof(wasteFactor), "Waste factor must be a finite number.");

        if (wasteFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(wasteFactor), "Waste factor cannot be negative.");

        if (spacingInches.TotalInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(spacingInches), "Spacing must be greater than zero.");

        if (wallLengths.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(wallLengths), "At least one wall length is required.");

        var studsPerWall = new int[wallLengths.Count];
        var baseStuds = 0;

        for (var i = 0; i < wallLengths.Count; i++)
        {
            var len = wallLengths[i];
            if (len.TotalInches <= 0)
                throw new ArgumentOutOfRangeException(nameof(wallLengths), "Wall lengths must be greater than zero.");

            // studs = ceil(length / spacing) + 1
            var raw = len.TotalInches / spacingInches.TotalInches;
            var studs = (int)Math.Ceiling(raw) + 1;

            studsPerWall[i] = studs;
            baseStuds += studs;
        }

        var totalStuds = baseStuds <= 0
            ? 0
            : (int)Math.Ceiling(baseStuds * (1.0 + wasteFactor));

        return new StudTakeoffResult(spacingInches, wasteFactor, baseStuds, totalStuds, studsPerWall);
    }
}
