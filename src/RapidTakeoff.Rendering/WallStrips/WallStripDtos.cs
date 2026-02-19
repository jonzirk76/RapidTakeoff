using RapidTakeoff.Rendering.Walls;

namespace RapidTakeoff.Rendering.WallStrips;

/// <summary>
/// Render-ready data contract for the Wall Strip SVG output.
/// Units are explicitly in feet and square-feet to keep rendering deterministic and dumb.
/// </summary>
/// <param name="ProjectName">Project display name.</param>
/// <param name="HeightFeet">Common wall height in feet.</param>
/// <param name="Walls">Independent wall segments to render as horizontal strips.</param>
/// <param name="Summary">Precomputed totals for the summary panel.</param>
/// <param name="Assumptions">Optional assumptions to render beside the summary panel.</param>
public sealed record WallStripDto(
    string ProjectName,
    double HeightFeet,
    IReadOnlyList<WallSegmentDto> Walls,
    SummaryDto Summary,
    IReadOnlyList<string>? Assumptions = null
);

/// <summary>
/// A single wall segment for strip rendering.
/// </summary>
/// <param name="Name">Wall label (e.g., "Wall A").</param>
/// <param name="LengthFeet">Wall length in feet.</param>
/// <param name="Penetrations">Penetrations/openings in wall-local coordinates.</param>
public sealed record WallSegmentDto(
    string Name,
    double LengthFeet,
    IReadOnlyList<PenetrationDto> Penetrations
);

/// <summary>
/// Precomputed project totals for inclusion in the SVG summary panel.
/// </summary>
/// <param name="TotalLengthFeet">Sum of all wall lengths in feet.</param>
/// <param name="NetAreaSqFt">Net area in square-feet (typically total length * height, minus openings if applicable).</param>
/// <param name="DrywallSheets">Drywall sheet count.</param>
/// <param name="StudCount">Stud count.</param>
/// <param name="InsulationUnits">Insulation units (whatever the current calculator outputs).</param>
public sealed record SummaryDto(
    double TotalLengthFeet,
    double NetAreaSqFt,
    int DrywallSheets,
    int StudCount,
    int InsulationUnits
);
