# Brief — Units & Rounding Policy

- Canonical length type is `RapidTakeoff.Core.Units.Length` storing inches in `TotalInches`.
- Enforce invariants everywhere: finite and non-negative (no negatives allowed).
- Add formatting + parsing as the single source of truth for construction units:
  - Architectural feet–inches–fractions, rounded to nearest 1/16".
  - Engineering feet + decimal inches.
  - Decimal and Scientific output in in/ft/mm/cm/m.
- All rounding must use MidpointRounding.AwayFromZero and occur only at formatting (and parse normalization).
- Add unit tests for invariants, conversions, formatting (incl carry/reduce), parsing, and invalid inputs.
- Keep scope minimal: do not refactor all project/input models or serialization.