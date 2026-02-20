using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Studs;

/// <summary>
/// Stud takeoff result with framing-category breakdown.
/// </summary>
public sealed class FramedStudTakeoffResult : StudTakeoffResult
{
    /// <summary>
    /// Gets the count of common studs (regular layout studs outside framed opening zones).
    /// </summary>
    public int CommonStuds { get; }

    /// <summary>
    /// Gets the count of king studs.
    /// </summary>
    public int KingStuds { get; }

    /// <summary>
    /// Gets the count of trimmer studs.
    /// </summary>
    public int TrimmerStuds { get; }

    /// <summary>
    /// Gets the count of top cripple studs.
    /// </summary>
    public int CrippleTopStuds { get; }

    /// <summary>
    /// Gets the count of bottom cripple studs.
    /// </summary>
    public int CrippleBottomStuds { get; }

    /// <summary>
    /// Initializes a new framed stud takeoff result.
    /// </summary>
    public FramedStudTakeoffResult(
        Length spacing,
        double wasteFactor,
        int baseStuds,
        int totalStuds,
        IReadOnlyList<int> studsPerWall,
        int commonStuds,
        int kingStuds,
        int trimmerStuds,
        int crippleTopStuds,
        int crippleBottomStuds)
        : base(spacing, wasteFactor, baseStuds, totalStuds, studsPerWall)
    {
        CommonStuds = commonStuds;
        KingStuds = kingStuds;
        TrimmerStuds = trimmerStuds;
        CrippleTopStuds = crippleTopStuds;
        CrippleBottomStuds = crippleBottomStuds;
    }
}
