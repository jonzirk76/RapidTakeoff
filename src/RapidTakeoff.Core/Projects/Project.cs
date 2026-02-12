namespace RapidTakeoff.Core.Projects;

/// <summary>
/// Represents a simple project definition used for takeoff calculations.
/// </summary>
public sealed class Project
{
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets wall height in feet.
    /// </summary>
    public double WallHeightFeet { get; set; }

    /// <summary>
    /// Gets or sets wall lengths in feet.
    /// </summary>
    public double[] WallLengthsFeet { get; set; } = [];

    /// <summary>
    /// Gets or sets project-level takeoff settings.
    /// </summary>
    public ProjectSettings Settings { get; set; } = new();

    /// <summary>
    /// Validates project values and throws when invalid.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentOutOfRangeException(nameof(Name), "Project name is required.");

        ValidateFiniteNonNegative(WallHeightFeet, nameof(WallHeightFeet));

        if (WallLengthsFeet is null)
            throw new ArgumentNullException(nameof(WallLengthsFeet), "Wall lengths are required.");

        if (WallLengthsFeet.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(WallLengthsFeet), "At least one wall length is required.");

        for (var i = 0; i < WallLengthsFeet.Length; i++)
        {
            ValidateFiniteNonNegative(WallLengthsFeet[i], nameof(WallLengthsFeet));
        }

        ArgumentNullException.ThrowIfNull(Settings);
        Settings.Validate();
    }

    private static void ValidateFiniteNonNegative(double value, string paramName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be a finite number.");
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} cannot be negative.");
    }
}
