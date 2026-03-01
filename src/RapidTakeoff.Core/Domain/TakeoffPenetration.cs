using RapidTakeoff.Core.Units;

namespace RapidTakeoff.Core.Domain;

/// <summary>
/// Native penetration/opening model using canonical units.
/// </summary>
public sealed class TakeoffPenetration
{
    /// <summary>
    /// Gets the penetration identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the penetration type token.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the zero-based wall index.
    /// </summary>
    public required int WallIndex { get; init; }

    /// <summary>
    /// Gets the left-edge offset from wall start.
    /// </summary>
    public required Length X { get; init; }

    /// <summary>
    /// Gets the bottom-edge offset from floor.
    /// </summary>
    public required Length Y { get; init; }

    /// <summary>
    /// Gets the opening width.
    /// </summary>
    public required Length Width { get; init; }

    /// <summary>
    /// Gets the opening height.
    /// </summary>
    public required Length Height { get; init; }
}
