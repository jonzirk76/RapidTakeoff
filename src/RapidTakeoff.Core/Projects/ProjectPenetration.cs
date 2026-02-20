namespace RapidTakeoff.Core.Projects;

/// <summary>
/// Represents a penetration/opening in a wall for net-area calculations.
/// Coordinates are wall-local in feet.
/// </summary>
public sealed class ProjectPenetration
{
    /// <summary>
    /// Gets or sets the penetration identifier (e.g., WIN-01).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the penetration type (e.g., window, door).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zero-based wall index this penetration belongs to.
    /// </summary>
    public int WallIndex { get; set; }

    /// <summary>
    /// Gets or sets the left-edge X offset from wall start in feet.
    /// </summary>
    public double XFeet { get; set; }

    /// <summary>
    /// Gets or sets the bottom-edge Y offset from floor in feet.
    /// </summary>
    public double YFeet { get; set; }

    /// <summary>
    /// Gets or sets the penetration width in feet.
    /// </summary>
    public double WidthFeet { get; set; }

    /// <summary>
    /// Gets or sets the penetration height in feet.
    /// </summary>
    public double HeightFeet { get; set; }
}
