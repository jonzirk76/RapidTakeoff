using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Drywall;

/// <summary>
/// Calculates sheet quantity for drywall takeoff based on area, sheet size, and waste.
/// </summary>
public static class DrywallTakeoffCalculator
{
    /// <summary>
    /// Calculates the required number of sheets.
    /// </summary>
    /// <param name="netArea">Net area before waste (non-negative).</param>
    /// <param name="sheet">Sheet size to use.</param>
    /// <param name="wasteFactor">
    /// Waste factor as a fraction (e.g., 0.10 for 10%). Must be >= 0.
    /// </param>
    public static DrywallTakeoffResult Calculate(Area netArea, DrywallSheet sheet, double wasteFactor)
    {
        if (double.IsNaN(wasteFactor) || double.IsInfinity(wasteFactor))
            throw new ArgumentOutOfRangeException(nameof(wasteFactor), "Waste factor must be a finite number.");

        if (wasteFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(wasteFactor), "Waste factor cannot be negative.");

        var grossArea = netArea * (1.0 + wasteFactor);

        // Always ceiling sheets. We treat 0 area as 0 sheets.
        var sheetAreaSqFt = sheet.Area.TotalSquareFeet;
        if (sheetAreaSqFt <= 0)
            throw new InvalidOperationException("Sheet area must be greater than zero.");

        var rawSheets = grossArea.TotalSquareFeet / sheetAreaSqFt;
        var sheetCount = rawSheets <= 0 ? 0 : (int)Math.Ceiling(rawSheets);

        return new DrywallTakeoffResult(netArea, grossArea, sheet, wasteFactor, sheetCount);
    }
}
