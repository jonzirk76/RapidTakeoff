using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Drywall;

/// <summary>
/// Result of a drywall sheet takeoff calculation.
/// </summary>
public sealed class DrywallTakeoffResult
{
    /// <summary>
    /// Gets the net area before waste.
    /// </summary>
    public Area NetArea { get; }

    /// <summary>
    /// Gets the gross area after applying waste.
    /// </summary>
    public Area GrossArea { get; }

    /// <summary>
    /// Gets the sheet used for calculation.
    /// </summary>
    public DrywallSheet Sheet { get; }

    /// <summary>
    /// Gets the waste factor used (e.g., 0.10 for 10%).
    /// </summary>
    public double WasteFactor { get; }

    /// <summary>
    /// Gets the resulting sheet count (ceiling).
    /// </summary>
    public int SheetCount { get; }

    /// <summary>
    /// Initializes a new takeoff result.
    /// </summary>
    public DrywallTakeoffResult(Area netArea, Area grossArea, DrywallSheet sheet, double wasteFactor, int sheetCount)
    {
        NetArea = netArea;
        GrossArea = grossArea;
        Sheet = sheet;
        WasteFactor = wasteFactor;
        SheetCount = sheetCount;
    }
}
