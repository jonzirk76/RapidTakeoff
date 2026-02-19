namespace RapidTakeoff.Core.Projects;

/// <summary>
/// Configurable defaults for project takeoff calculations.
/// </summary>
public sealed class ProjectSettings
{
    /// <summary>
    /// Gets or sets the drywall sheet token. Supported values are <c>4x8</c> and <c>4x12</c>.
    /// </summary>
    public string DrywallSheet { get; set; } = "4x8";

    /// <summary>
    /// Gets or sets the drywall waste factor as a fraction (e.g., 0.10 for 10%).
    /// </summary>
    public double DrywallWaste { get; set; } = 0.10;

    /// <summary>
    /// Gets or sets stud spacing in inches.
    /// </summary>
    public double StudsSpacingInches { get; set; } = 16.0;

    /// <summary>
    /// Gets or sets the stud waste factor as a fraction (e.g., 0.10 for 10%).
    /// </summary>
    public double StudsWaste { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the nominal stud type token (e.g., <c>2x4</c>, <c>2x6</c>).
    /// </summary>
    public string StudType { get; set; } = "2x4";

    /// <summary>
    /// Gets or sets whether studs whose centerlines fall within penetration spans should be removed.
    /// </summary>
    public bool StudsSubtractPenetrations { get; set; } = false;

    /// <summary>
    /// Gets or sets the insulation waste factor as a fraction (e.g., 0.10 for 10%).
    /// </summary>
    public double InsulationWaste { get; set; } = 0.10;

    /// <summary>
    /// Gets or sets insulation coverage per roll or bag in square feet.
    /// </summary>
    public double InsulationCoverageSquareFeet { get; set; } = 40.0;

    /// <summary>
    /// Validates settings and throws when invalid.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DrywallSheet))
            throw new ArgumentOutOfRangeException(nameof(DrywallSheet), "Drywall sheet is required.");

        var normalizedSheet = DrywallSheet.Trim().ToLowerInvariant();
        if (normalizedSheet is not ("4x8" or "4x12"))
            throw new ArgumentOutOfRangeException(nameof(DrywallSheet), "Drywall sheet must be '4x8' or '4x12'.");

        if (string.IsNullOrWhiteSpace(StudType))
            throw new ArgumentOutOfRangeException(nameof(StudType), "Stud type is required.");

        var normalizedStudType = StudType.Trim().ToLowerInvariant();
        if (normalizedStudType is not ("2x4" or "2x6" or "2x8" or "2x10" or "2x12"))
            throw new ArgumentOutOfRangeException(nameof(StudType), "Stud type must be one of: 2x4, 2x6, 2x8, 2x10, 2x12.");

        ValidateFiniteNonNegative(DrywallWaste, nameof(DrywallWaste));
        ValidateFiniteNonNegative(StudsWaste, nameof(StudsWaste));
        ValidateFiniteNonNegative(InsulationWaste, nameof(InsulationWaste));

        ValidateFinitePositive(StudsSpacingInches, nameof(StudsSpacingInches));
        ValidateFinitePositive(InsulationCoverageSquareFeet, nameof(InsulationCoverageSquareFeet));

        DrywallSheet = normalizedSheet;
        StudType = normalizedStudType;
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
}
