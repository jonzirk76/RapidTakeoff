using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Insulation;

/// <summary>
/// Result of an insulation takeoff calculation.
/// </summary>
public sealed class InsulationTakeoffResult
{
    /// <summary>
    /// Gets the net area before waste.
    /// </summary>
    public Area NetArea { get; }

    /// <summary>
    /// Gets the gross area after waste.
    /// </summary>
    public Area GrossArea { get; }

    /// <summary>
    /// Gets the product used for the calculation.
    /// </summary>
    public InsulationProduct Product { get; }

    /// <summary>
    /// Gets the waste factor used (e.g., 0.10 for 10%).
    /// </summary>
    public double WasteFactor { get; }

    /// <summary>
    /// Gets the resulting quantity in rolls/bags (ceiling).
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Initializes a new insulation takeoff result.
    /// </summary>
    public InsulationTakeoffResult(Area netArea, Area grossArea, InsulationProduct product, double wasteFactor, int quantity)
    {
        NetArea = netArea;
        GrossArea = grossArea;
        Product = product;
        WasteFactor = wasteFactor;
        Quantity = quantity;
    }
}
