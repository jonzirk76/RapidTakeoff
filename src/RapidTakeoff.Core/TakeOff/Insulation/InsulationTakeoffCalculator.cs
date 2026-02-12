using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Insulation;

/// <summary>
/// Calculates insulation quantities based on area, product coverage, and waste.
/// </summary>
public static class InsulationTakeoffCalculator
{
    /// <summary>
    /// Calculates insulation quantity (rolls/bags) for a net area.
    /// </summary>
    /// <param name="netArea">Net area before waste.</param>
    /// <param name="product">Insulation product with per-unit coverage.</param>
    /// <param name="wasteFactor">Waste factor as a fraction (e.g., 0.10 for 10%). Must be >= 0.</param>
    public static InsulationTakeoffResult Calculate(Area netArea, InsulationProduct product, double wasteFactor)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (double.IsNaN(wasteFactor) || double.IsInfinity(wasteFactor))
            throw new ArgumentOutOfRangeException(nameof(wasteFactor), "Waste factor must be a finite number.");

        if (wasteFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(wasteFactor), "Waste factor cannot be negative.");

        var grossArea = netArea * (1.0 + wasteFactor);
        var coverageSqFt = product.CoverageArea.TotalSquareFeet;

        if (coverageSqFt <= 0)
            throw new InvalidOperationException("Coverage area must be greater than zero.");

        var rawQty = grossArea.TotalSquareFeet / coverageSqFt;
        var quantity = rawQty <= 0 ? 0 : (int)Math.Ceiling(rawQty);

        return new InsulationTakeoffResult(netArea, grossArea, product, wasteFactor, quantity);
    }
}
