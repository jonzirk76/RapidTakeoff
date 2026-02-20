namespace RapidTakeoff.Rendering.Walls;

/// <summary>
/// A void region within a wall (e.g., window/door) expressed in wall-local coordinates.
/// X is along wall length (0..LengthFeet), Y is vertical (0..HeightFeet).
/// </summary>
/// <param name="Id">Identifier (e.g., WIN-01).</param>
/// <param name="Type">Type (e.g., window, door).</param>
/// <param name="XFeet">Left edge offset from wall start.</param>
/// <param name="YFeet">Bottom edge offset from floor.</param>
/// <param name="WidthFeet">Width of the void.</param>
/// <param name="HeightFeet">Height of the void.</param>
public sealed record PenetrationDto(
    string Id,
    string Type,
    double XFeet,
    double YFeet,
    double WidthFeet,
    double HeightFeet
);
