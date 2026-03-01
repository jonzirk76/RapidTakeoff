namespace RapidTakeoff.Core.Units;

/// <summary>
/// Represents a non-negative length measurement.
/// Internally stored in inches to align with common residential takeoff inputs
/// (stud spacing, sheet dimensions, etc.).
/// </summary>
public readonly record struct Length
{
    private const int ArchitecturalDenominator = 16;
    private const int InchesPerFoot = 12;
    private const double MillimetersPerInch = 25.4;

    /// <summary>
    /// Gets the total length in inches.
    /// </summary>
    public double TotalInches { get; }

    /// <summary>
    /// Gets the total length in feet.
    /// </summary>
    public double TotalFeet => TotalInches / InchesPerFoot;

    private Length(double totalInches)
    {
        ValidateFiniteNonNegative(totalInches, nameof(totalInches), "Length");
        TotalInches = totalInches;
    }

    /// <summary>
    /// Creates a <see cref="Length"/> from inches.
    /// </summary>
    /// <param name="inches">Total inches (must be finite and non-negative).</param>
    public static Length FromInches(double inches)
    {
        ValidateFiniteNonNegative(inches, nameof(inches), "Inches");
        return new(inches);
    }

    /// <summary>
    /// Creates a <see cref="Length"/> from feet.
    /// </summary>
    /// <param name="feet">Total feet (must be finite and non-negative).</param>
    public static Length FromFeet(double feet)
    {
        ValidateFiniteNonNegative(feet, nameof(feet), "Feet");
        return new(feet * InchesPerFoot);
    }

    /// <summary>
    /// Creates a <see cref="Length"/> from a feet + inches pair.
    /// </summary>
    /// <param name="feet">Feet portion (must be finite and non-negative).</param>
    /// <param name="inches">Inches portion (must be finite and non-negative).</param>
    public static Length FromFeetAndInches(double feet, double inches)
    {
        ValidateFiniteNonNegative(feet, nameof(feet), "Feet");
        ValidateFiniteNonNegative(inches, nameof(inches), "Inches");
        return new Length((feet * InchesPerFoot) + inches);
    }

    /// <summary>
    /// Creates a <see cref="Length"/> from a value in the given unit basis.
    /// </summary>
    public static Length From(double value, UnitBasis basis)
    {
        ValidateFiniteNonNegative(value, nameof(value), "Value");
        return basis switch
        {
            UnitBasis.Inches => FromInches(value),
            UnitBasis.Feet => FromFeet(value),
            UnitBasis.Millimeters => FromInches(value / MillimetersPerInch),
            UnitBasis.Centimeters => FromInches((value * 10.0) / MillimetersPerInch),
            UnitBasis.Meters => FromInches((value * 1000.0) / MillimetersPerInch),
            _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "Unsupported unit basis.")
        };
    }

    /// <summary>
    /// Adds two lengths.
    /// </summary>
    public static Length operator +(Length a, Length b) => new(a.TotalInches + b.TotalInches);

    /// <summary>
    /// Subtracts one length from another. Result must be non-negative.
    /// </summary>
    public static Length operator -(Length a, Length b) => new(a.TotalInches - b.TotalInches);

    /// <summary>
    /// Scales a length by a factor. Result must be non-negative.
    /// </summary>
    public static Length operator *(Length a, double factor) => new(a.TotalInches * factor);

    /// <summary>
    /// Scales a length by a factor. Result must be non-negative.
    /// </summary>
    public static Length operator *(double factor, Length a) => a * factor;

    /// <summary>
    /// Divides a length by a divisor. Result must be non-negative.
    /// </summary>
    public static Length operator /(Length a, double divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Divisor cannot be zero.");
        return new Length(a.TotalInches / divisor);
    }

    /// <summary>
    /// Converts this length to the requested unit basis.
    /// </summary>
    public double To(UnitBasis basis)
    {
        return basis switch
        {
            UnitBasis.Inches => TotalInches,
            UnitBasis.Feet => TotalFeet,
            UnitBasis.Millimeters => TotalInches * MillimetersPerInch,
            UnitBasis.Centimeters => (TotalInches * MillimetersPerInch) / 10.0,
            UnitBasis.Meters => (TotalInches * MillimetersPerInch) / 1000.0,
            _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "Unsupported unit basis.")
        };
    }

    /// <summary>
    /// Formats the length in the requested style and basis.
    /// </summary>
    public string Format(UnitStyle style, UnitBasis basis = UnitBasis.Inches, int precision = 3)
    {
        if (precision < 0)
            throw new ArgumentOutOfRangeException(nameof(precision), "Precision cannot be negative.");

        return style switch
        {
            UnitStyle.Architectural => FormatArchitectural(),
            UnitStyle.Engineering => FormatEngineering(precision),
            UnitStyle.Decimal => $"{FormatDecimal(To(basis), precision)} {GetUnitSuffix(basis)}",
            UnitStyle.Scientific => $"{Math.Round(To(basis), precision, MidpointRounding.AwayFromZero).ToString($"E{precision}", System.Globalization.CultureInfo.InvariantCulture)} {GetUnitSuffix(basis)}",
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Unsupported unit style.")
        };
    }

    /// <summary>
    /// Parses a length from a supported string format.
    /// </summary>
    public static Length Parse(string text)
    {
        if (!TryParse(text, out var length))
            throw new FormatException($"Unable to parse length '{text}'.");
        return length;
    }

    /// <summary>
    /// Tries to parse a length from common construction formats.
    /// </summary>
    public static bool TryParse(string? text, out Length length)
    {
        length = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var input = text.Trim();
        if (input.StartsWith("-", StringComparison.Ordinal))
            return false;

        if (TryParseFeetAndInchesWords(input, out var feet, out var inches))
        {
            length = FromInches((feet * InchesPerFoot) + inches);
            return true;
        }

        if (TryParseArchitectural(input, out length))
            return true;

        if ((input.Contains('"') || input.Contains('/')) && TryParseInchesComponent(input, out var inchOnly))
        {
            length = FromInches(RoundToNearestArchitecturalFraction(inchOnly));
            return true;
        }

        if (TryParseValueWithUnit(input, out var value, out var basis))
        {
            length = From(value, basis);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a human-friendly representation in feet and inches.
    /// </summary>
    public override string ToString()
    {
        // Keep it simple and non-rounding-opinionated for now.
        return $"{TotalFeet:0.###} ft ({TotalInches:0.###} in)";
    }

    private string FormatArchitectural()
    {
        var sixteenths = (int)Math.Round(TotalInches * ArchitecturalDenominator, 0, MidpointRounding.AwayFromZero);
        var sixteenthsPerFoot = InchesPerFoot * ArchitecturalDenominator;
        var feet = sixteenths / sixteenthsPerFoot;
        var remainder = sixteenths % sixteenthsPerFoot;
        var wholeInches = remainder / ArchitecturalDenominator;
        var fraction = remainder % ArchitecturalDenominator;

        if (fraction == 0)
            return $"{feet}'-{wholeInches}\"";

        var divisor = Gcd(fraction, ArchitecturalDenominator);
        var numerator = fraction / divisor;
        var denominator = ArchitecturalDenominator / divisor;

        if (wholeInches == 0)
            return $"{feet}'-{numerator}/{denominator}\"";

        return $"{feet}'-{wholeInches} {numerator}/{denominator}\"";
    }

    private string FormatEngineering(int precision)
    {
        var feet = (int)Math.Floor(TotalInches / InchesPerFoot);
        var inches = TotalInches - (feet * InchesPerFoot);
        var roundedInches = Math.Round(inches, precision, MidpointRounding.AwayFromZero);

        if (roundedInches >= InchesPerFoot)
        {
            feet += (int)Math.Floor(roundedInches / InchesPerFoot);
            roundedInches %= InchesPerFoot;
        }

        return $"{feet}'-{FormatDecimal(roundedInches, precision)}\"";
    }

    private static bool TryParseArchitectural(string input, out Length length)
    {
        length = default;

        var quoteIndex = input.IndexOf('\'');
        if (quoteIndex < 0)
            return false;

        var feetPart = input[..quoteIndex].Trim();
        if (feetPart.StartsWith("+", StringComparison.Ordinal))
            feetPart = feetPart[1..].Trim();

        if (!TryParseNonNegativeNumber(feetPart, out var feet))
            return false;

        var remainder = input[(quoteIndex + 1)..].TrimStart();
        remainder = remainder.TrimStart('-', ' ');

        if (string.IsNullOrWhiteSpace(remainder))
        {
            length = FromFeet(feet);
            return true;
        }

        if (!TryParseInchesComponent(remainder, out var inches))
            return false;

        var normalizedInches = RoundToNearestArchitecturalFraction((feet * InchesPerFoot) + inches);
        length = FromInches(normalizedInches);
        return true;
    }

    private static bool TryParseFeetAndInchesWords(string input, out double feet, out double inches)
    {
        feet = 0;
        inches = 0;

        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length < 2 || tokens.Length > 4)
            return false;

        var feetToken = tokens[0];
        var feetUnit = tokens[1].ToLowerInvariant();
        if (feetUnit is not ("ft" or "foot" or "feet"))
            return false;

        if (!TryParseNonNegativeNumber(feetToken.TrimStart('+'), out feet))
            return false;

        if (tokens.Length == 2)
            return true;

        var inchesToken = tokens[2];
        if (!TryParseNonNegativeNumber(inchesToken, out inches))
            return false;

        if (tokens.Length == 3)
            return true;

        var inchUnit = tokens[3].ToLowerInvariant();
        return inchUnit is "in" or "inch" or "inches";
    }

    private static bool TryParseInchesComponent(string input, out double inches)
    {
        inches = 0;
        var normalized = input.Trim();
        if (normalized.Length == 0)
            return false;

        if (normalized.EndsWith("\"", StringComparison.Ordinal))
            normalized = normalized[..^1].TrimEnd();

        normalized = TrimWordSuffix(normalized, "inches");
        normalized = TrimWordSuffix(normalized, "inch");
        normalized = TrimWordSuffix(normalized, "in");
        normalized = normalized.Trim();

        if (normalized.Length == 0)
            return false;

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length > 2)
            return false;

        if (parts.Length == 1)
        {
            return TryParseNumberOrFraction(parts[0], out inches);
        }

        if (!TryParseNonNegativeNumber(parts[0], out var whole))
            return false;
        if (!TryParseFraction(parts[1], out var fraction))
            return false;

        inches = whole + fraction;
        return true;
    }

    private static bool TryParseNumberOrFraction(string token, out double value)
    {
        if (TryParseNonNegativeNumber(token, out value))
            return true;
        return TryParseFraction(token, out value);
    }

    private static bool TryParseFraction(string token, out double value)
    {
        value = 0;
        var split = token.Split('/', StringSplitOptions.TrimEntries);
        if (split.Length != 2)
            return false;
        if (!int.TryParse(split[0], out var numerator) || numerator < 0)
            return false;
        if (!int.TryParse(split[1], out var denominator) || denominator <= 0)
            return false;

        value = (double)numerator / denominator;
        return true;
    }

    private static bool TryParseValueWithUnit(string input, out double value, out UnitBasis basis)
    {
        value = 0;
        basis = UnitBasis.Inches;

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 2)
            return false;

        var valueToken = parts[0];
        if (valueToken.StartsWith("+", StringComparison.Ordinal))
            valueToken = valueToken[1..];

        if (!double.TryParse(
            valueToken,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out value))
        {
            return false;
        }

        if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
            return false;

        if (parts.Length == 1)
        {
            basis = UnitBasis.Inches;
            return true;
        }

        var unit = parts[1].ToLowerInvariant();
        return TryMapUnitBasis(unit, out basis);
    }

    private static bool TryMapUnitBasis(string unit, out UnitBasis basis)
    {
        basis = unit switch
        {
            "in" or "inch" or "inches" => UnitBasis.Inches,
            "ft" or "foot" or "feet" => UnitBasis.Feet,
            "mm" or "millimeter" or "millimeters" => UnitBasis.Millimeters,
            "cm" or "centimeter" or "centimeters" => UnitBasis.Centimeters,
            "m" or "meter" or "meters" => UnitBasis.Meters,
            _ => UnitBasis.Inches
        };

        return unit is
            "in" or "inch" or "inches" or
            "ft" or "foot" or "feet" or
            "mm" or "millimeter" or "millimeters" or
            "cm" or "centimeter" or "centimeters" or
            "m" or "meter" or "meters";
    }

    private static string GetUnitSuffix(UnitBasis basis)
    {
        return basis switch
        {
            UnitBasis.Inches => "in",
            UnitBasis.Feet => "ft",
            UnitBasis.Millimeters => "mm",
            UnitBasis.Centimeters => "cm",
            UnitBasis.Meters => "m",
            _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "Unsupported unit basis.")
        };
    }

    private static string FormatDecimal(double value, int precision)
    {
        var rounded = Math.Round(value, precision, MidpointRounding.AwayFromZero);
        var format = precision == 0
            ? "0"
            : $"0.{new string('#', precision)}";
        return rounded.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            var temp = a % b;
            a = b;
            b = temp;
        }

        return Math.Abs(a);
    }

    private static double RoundToNearestArchitecturalFraction(double totalInches)
    {
        return Math.Round(totalInches * ArchitecturalDenominator, 0, MidpointRounding.AwayFromZero) / ArchitecturalDenominator;
    }

    private static string TrimWordSuffix(string value, string suffix)
    {
        if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            return value[..^suffix.Length].TrimEnd();
        return value;
    }

    private static bool TryParseNonNegativeNumber(string token, out double value)
    {
        var ok = double.TryParse(
            token,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
        if (!ok)
            return false;
        return value >= 0 && !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static void ValidateFiniteNonNegative(double value, string paramName, string label)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(paramName, $"{label} must be a finite number.");
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"{label} cannot be negative.");
    }
}
