using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Studs;

/// <summary>
/// Result of a stud takeoff calculation.
/// </summary>
public sealed class StudTakeoffResult
{
    /// <summary>
    /// Gets the stud spacing used (inches).
    /// </summary>
    public Length Spacing { get; }

    /// <summary>
    /// Gets the waste factor used (e.g., 0.10 for 10%).
    /// </summary>
    public double WasteFactor { get; }

    /// <summary>
    /// Gets the total studs before waste is applied.
    /// </summary>
    public int BaseStuds { get; }

    /// <summary>
    /// Gets the total studs after waste is applied (ceiling).
    /// </summary>
    public int TotalStuds { get; }

    /// <summary>
    /// Gets studs per wall (same order as the input wall list).
    /// </summary>
    public IReadOnlyList<int> StudsPerWall { get; }

    /// <summary>
    /// Initializes a new takeoff result.
    /// </summary>
    public StudTakeoffResult(Length spacing, double wasteFactor, int baseStuds, int totalStuds, IReadOnlyList<int> studsPerWall)
    {
        Spacing = spacing;
        WasteFactor = wasteFactor;
        BaseStuds = baseStuds;
        TotalStuds = totalStuds;
        StudsPerWall = studsPerWall;
    }
}
