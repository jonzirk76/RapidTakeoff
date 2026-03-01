using RapidTakeoff.Core.Domain;

namespace RapidTakeoff.Core.Projects;

/// <summary>
/// Represents a JSON project definition used for ingestion.
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
    /// Gets or sets wall penetrations/openings used to compute net wall area.
    /// </summary>
    public ProjectPenetration[] Penetrations { get; set; } = [];

    /// <summary>
    /// Validates project values and throws when invalid.
    /// </summary>
    public void Validate()
    {
        _ = ProjectNormalizer.Normalize(this);
    }

    /// <summary>
    /// Gets the gross wall area in square feet before penetration deductions.
    /// </summary>
    public double GetGrossWallAreaSquareFeet()
    {
        return ToTakeoffProject().GetGrossWallAreaSquareFeet();
    }

    /// <summary>
    /// Gets the total penetration/opening area in square feet, merged per wall so overlaps are not double-counted.
    /// </summary>
    public double GetPenetrationAreaSquareFeet()
    {
        return ToTakeoffProject().GetPenetrationAreaSquareFeet();
    }

    /// <summary>
    /// Gets the net wall area in square feet after penetration deductions.
    /// </summary>
    public double GetNetWallAreaSquareFeet()
    {
        return ToTakeoffProject().GetNetWallAreaSquareFeet();
    }

    /// <summary>
    /// Returns friendly, non-fatal validation warnings.
    /// </summary>
    public IReadOnlyList<string> GetValidationWarnings()
    {
        return ToTakeoffProject().GetValidationWarnings();
    }

    /// <summary>
    /// Converts this JSON DTO to the normalized native project model.
    /// </summary>
    public TakeoffProject ToTakeoffProject() => ProjectNormalizer.Normalize(this);
}
