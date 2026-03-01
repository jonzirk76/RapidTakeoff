namespace RapidTakeoff.Core.Units;

/// <summary>
/// Controls how a <see cref="Length"/> is formatted.
/// </summary>
public enum UnitStyle
{
    /// <summary>
    /// Feet-inches with fractional inches (nearest 1/16").
    /// </summary>
    Architectural,
    /// <summary>
    /// Feet-inches with decimal inches.
    /// </summary>
    Engineering,
    /// <summary>
    /// Decimal value in the selected basis.
    /// </summary>
    Decimal,
    /// <summary>
    /// Scientific notation in the selected basis.
    /// </summary>
    Scientific
}
