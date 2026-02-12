using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Takeoff.Insulation;

/// <summary>
/// Represents an insulation product with a known coverage area per unit.
/// </summary>
public sealed class InsulationProduct
{
    /// <summary>
    /// Gets the optional display name of the product.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the coverage area per roll or bag.
    /// </summary>
    public Area CoverageArea { get; }

    /// <summary>
    /// Initializes a new insulation product.
    /// </summary>
    /// <param name="coverageArea">Coverage area per roll/bag.</param>
    /// <param name="name">Optional product name.</param>
    public InsulationProduct(Area coverageArea, string? name = null)
    {
        if (coverageArea.TotalSquareInches <= 0)
            throw new ArgumentOutOfRangeException(nameof(coverageArea), "Coverage area must be greater than zero.");

        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        CoverageArea = coverageArea;
    }
}
