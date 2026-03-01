# Ticket: Units & Rounding Policy (Length)

## 1. Goal
Establish a single, consistent units/rounding policy for `RapidTakeoff.Core.Units.Length` (canonical inches) and provide robust formatting + parsing for common construction unit styles. Enforce invariants (finite, non-negative) so negative lengths cannot be constructed or parsed. Architectural output must round to the nearest 1/16".

## 2. Non-Goals
- Do NOT refactor all project/input models (e.g., `Project`, `ProjectPenetration`) to use `Length` end-to-end in this ticket.
- Do NOT change serialization formats or JSON schemas.
- Do NOT add new CLI commands/flags unrelated to length parsing/formatting.
- Do NOT introduce a database, server, or website changes.
- Do NOT add rounding logic outside of `RapidTakeoff.Core.Units` (except tests).

## 3. Current State (as observed)
- Canonical type exists: `public readonly record struct Length`
- Canonical storage: `public double TotalInches { get; }`
- Convenience: `public double TotalFeet => TotalInches / 12.0;`
- Factories:
  - `FromInches(double inches) => new(inches);` (no validation today)
  - `FromFeet(double feet) => new(feet * 12.0);` (no validation today)
  - `FromFeetAndInches(double feet, double inches)` validates finite + non-negative

## 4. Requirements

### 4.1 Invariants (must be enforced everywhere)
A `Length` must always be:
- Finite (`!NaN`, `!Infinity`)
- Non-negative (`>= 0`)

#### 4.1.1 Constructor enforcement
Add an explicit constructor to `Length` so `new Length(x)` cannot bypass invariants.

Required behavior:
- If `totalInches` is NaN or Infinity: throw `ArgumentOutOfRangeException`
- If `totalInches < 0`: throw `ArgumentOutOfRangeException`

#### 4.1.2 Factory method enforcement
Update factories to enforce invariants:
- `FromInches(double inches)` validates finite + non-negative
- `FromFeet(double feet)` validates finite + non-negative before conversion
- `FromFeetAndInches(...)` already validates; keep it, but ensure it routes through the invariant constructor (directly or indirectly)

### 4.2 Unit Styles and Bases (enums)
Add enums under `RapidTakeoff.Core.Units` namespace:

```csharp
public enum UnitStyle
{
    Architectural, // feet-inches-fractional inches
    Engineering,   // feet + decimal inches
    Decimal,       // decimal in chosen basis (in/ft/mm/cm/m)
    Scientific     // scientific notation in chosen basis
}

public enum UnitBasis
{
    Inches,
    Feet,
    Millimeters,
    Centimeters,
    Meters
}

4.3 Conversions (exhaustive/common)

Add conversion support for all UnitBasis values.

Required:

12 inches = 1 foot

1 inch = 25.4 mm (exact)

cm/m via mm

Conversions must reject negative/NaN/Infinity at input boundaries (via Length invariants)

Required API additions:

public double To(UnitBasis basis);

public static Length From(UnitBasis basis, double value);
4.4 Rounding Policy (single source of truth)

All rounding relevant to Length must use:

MidpointRounding.AwayFromZero

Rounding must happen ONLY at formatting boundaries (and parse normalization where appropriate), not during intermediate calculations.

4.5 Formatting APIs

Add formatting methods to Length:

public string ToArchitecturalString(int denom = 16);

public string ToEngineeringString(int decimalInches = 2);

public string ToDecimalString(
    UnitBasis basis = UnitBasis.Inches,
    int decimals = 2,
    bool includeUnitSuffix = true
);

public string ToScientificString(
    UnitBasis basis = UnitBasis.Inches,
    int significantDigits = 6,
    bool includeUnitSuffix = true
);

// Router convenience (optional but recommended)
public string Format(
    UnitStyle style = UnitStyle.Architectural,
    UnitBasis basis = UnitBasis.Inches,
    int precision = 2,
    int denom = 16,
    int significantDigits = 6,
    bool includeUnitSuffix = true
);
4.5.1 Architectural formatting rules

Output format: F'-I N/D" or F'-I" or F'-0"

Always include feet and inches.

Example: 10'-3 1/2", 10'-3", 10'-0"

Rounding: nearest 1/denom inch. Default denom = 16.

Normalize/carry:

If fraction rounds to denom/denom, increment inches and clear fraction.

If inches becomes 12, increment feet and set inches to 0.

Reduce fractions:

e.g. 8/16 -> 1/2, 2/16 -> 1/8

No negative sign ever appears.

4.5.2 Engineering formatting rules (feet + decimal inches)

Output format: F'-I.DD"

Feet is integer.

Inches is decimal with decimalInches places (default 2).

Inches must be normalized to [0, 12), carrying into feet as needed.

Rounding: AwayFromZero at decimalInches.

4.5.3 Decimal formatting rules

Output: <value> <unit>

Examples: 123.50 in, 10.29 ft, 3136.90 mm

Rounding: AwayFromZero at decimals

Unit suffixes:

in, ft, mm, cm, m

4.5.4 Scientific formatting rules

Output: <value> <unit> where value is in scientific notation

Example: 1.234567E+02 in

Significant digits: default 6

Unit suffixes as above

4.6 Parsing APIs

Add:

public static bool TryParse(string input, out Length length, out string? error);

public static Length Parse(string input);

Parsing must:

Reject negatives anywhere.

Be whitespace tolerant.

Be case-insensitive for unit suffixes.

4.6.1 Supported input forms (v0)

Architectural-ish:

10'-3 1/2"

10' 3 1/2"

10'3-1/2"

3 1/2"

7/16"

4" / 4 in

Engineering-ish:

10'-3.25"

3.25" / 3.25 in

Decimal with units:

10.5 ft

120 in

300 mm

30 cm

3 m

If the input has no explicit unit:

If it contains ' or " or /, parse as architectural/engineering feet/inches.

Otherwise parse as decimal inches.

4.7 Error handling

TryParse returns false and sets error to a helpful message.

Parse throws FormatException with a helpful message.

5. Test Matrix (must add)
5.1 Invariants

new Length(-1) throws ArgumentOutOfRangeException

Length.FromInches(-0.01) throws

Length.FromFeet(-0.01) throws

Length.Parse("-1\"") throws (or TryParse false)

5.2 Conversions

Length.FromInches(1).To(UnitBasis.Millimeters) == 25.4 (tolerance)

Length.FromFeet(1).TotalInches == 12.0

Round-trip sanity checks for a few values.

5.3 Architectural formatting (1/16")

Length.FromInches(123.5) => 10'-3 1/2"

Length.FromInches(120) => 10'-0"

Length.FromInches(0.03125) (1/32) rounds to 0'-0" at denom=16

Carry case:

a value that rounds to ... 12" must carry to next foot correctly (add at least one explicit test)

5.4 Engineering formatting

Length.FromInches(123.5).ToEngineeringString(2) => 10'-3.50"

A value whose inches part rounds to 12.00 must carry to feet.

5.5 Parsing

Add tests that each supported input parses to expected TotalInches (within tolerance):

10'-3 1/2"

10' 3 1/2"

10'3-1/2"

3 1/2"

7/16"

4"

10'-3.25"

10.5 ft

300 mm

Also add at least 3 invalid inputs:

negative

nonsense string

malformed fraction (e.g. 3/0")

6. Definition of Done

dotnet test passes

All new rounding uses MidpointRounding.AwayFromZero

Length invariants cannot be bypassed via new Length(...) or factories

Formatting/parsing covered by the test matrix

Add documentation file: docs/units-rounding.md summarizing:

canonical storage (inches)

supported formats

rounding rules

examples


---

## Suggested `brief.md` (optional, but nice)
If you want the folder to be complete, here’s a short `brief.md` you can drop in too:

```markdown
# Brief
Canonical length unit is inches (`RapidTakeoff.Core.Units.Length`). Negative lengths must be impossible to construct or parse. Add robust parsing and formatting for construction styles:

- Architectural: feet-inches-fractions rounded to 1/16"
- Engineering: feet + decimal inches
- Decimal: decimal value in in/ft/mm/cm/m
- Scientific: scientific notation in in/ft/mm/cm/m

All rounding must be MidpointRounding.AwayFromZero. Add exhaustive tests for rounding/carry and parsing edge cases.