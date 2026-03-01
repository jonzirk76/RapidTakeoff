# Ticket: Normalize Project inputs into native domain objects

## 1. Goal
Eliminate raw `double` length usage (feet-based fields) from the takeoff engine by introducing native domain models that use `RapidTakeoff.Core.Units.Length` and by performing a single normalization + validation step at ingestion. After this change, core calculators for `estimate` must operate on the normalized native project model (or on native lengths), not on raw feet doubles.

## 2. Non-Goals
- Do NOT change the JSON schema (existing `Project` JSON must still load).
- Do NOT rename JSON fields.
- Do NOT build a GUI or web app in this ticket.
- Do NOT refactor unrelated CLI commands (drywall/studs/insulation) unless they share code paths with estimate ingestion.
- Do NOT rewrite the renderer; only adapt the data passed into it as needed.
- Do NOT attempt a full redesign of project modeling (no room geometry/orientation yet).

## 3. Current State
Current types (used for JSON + internal use today):
```csharp
public sealed class Project
{
    public string Name { get; set; }
    public double WallHeightFeet { get; set; }
    public double[] WallLengthsFeet { get; set; }
    public ProjectSettings Settings { get; set; }
    public ProjectPenetration[] Penetrations { get; set; }
}

public sealed class ProjectPenetration
{
    public string Id { get; set; }
    public string Type { get; set; }
    public int WallIndex { get; set; }
    public double XFeet { get; set; }
    public double YFeet { get; set; }
    public double WidthFeet { get; set; }
    public double HeightFeet { get; set; }
}

These contain raw doubles in feet and therefore bypass the canonical Length policy.

4. Required Design
4.1 Introduce native models

Add new native types under RapidTakeoff.Core.Domain (or a consistent folder/namespace you prefer, but keep them in Core):

public sealed class TakeoffProject
{
    public required string Name { get; init; }

    public required Length WallHeight { get; init; }              // canonical
    public required IReadOnlyList<Length> WallLengths { get; init; }

    public required ProjectSettings Settings { get; init; }       // may remain as-is for now
    public required IReadOnlyList<TakeoffPenetration> Penetrations { get; init; }
}

public sealed class TakeoffPenetration
{
    public required string Id { get; init; }
    public required string Type { get; init; }

    public required int WallIndex { get; init; }

    public required Length X { get; init; }
    public required Length Y { get; init; }
    public required Length Width { get; init; }
    public required Length Height { get; init; }
}

Notes:

Keep ProjectSettings unchanged for now unless it contains other unit-bearing doubles that should also become native in this ticket. If it does contain unit-bearing fields (e.g., spacing inches), convert those too ONLY if they are clearly required for estimate path and are currently raw doubles/ints with unclear units.

4.2 Normalization/validation entry point

Add a normalizer responsible for:

converting feet-based doubles to Length

enforcing invariants + bounds checks in one place

returning the native model

Proposed API (exact name flexible, but must be single obvious entry):

public static class ProjectNormalizer
{
    public static TakeoffProject Normalize(Project raw);
}

Normalize(Project raw) must:

validate required fields and arrays are non-null

validate numeric values are finite (not NaN/Infinity)

validate no negatives for any length-bearing input

validate arrays have valid lengths (e.g., at least 1 wall length)

validate penetration WallIndex is in range

Conversion rules:

WallHeightFeet -> Length.FromFeet(...)

WallLengthsFeet[i] -> Length.FromFeet(...)

XFeet, YFeet, WidthFeet, HeightFeet -> Length.FromFeet(...)

4.3 Penetration bounds checks (must remain / move here)

Normalization must enforce strict bounds checks for penetrations:

0 <= WallIndex < WallLengths.Count

For each penetration:

0 <= X <= WallLength

0 <= Y <= WallHeight

Width > 0, Height > 0

X + Width <= WallLength

Y + Height <= WallHeight

If invalid: throw a validation exception with a helpful message (consistent with current CLI behavior: validation exceptions surface as CLI errors).

4.4 Overlap warnings (non-fatal)

If the current system has overlap warnings, keep behavior consistent:

Normalizer may detect overlaps and return them as warnings OR leave detection to existing code.

This ticket does not require building a new warning subsystem; it requires that overlap behavior does not regress.

If a warning mechanism already exists, route warnings through it. If not, do nothing in this ticket (but do not add overlap rejection).

5. Integration Requirements
5.1 Estimate pipeline must use native project

Update the estimate ingestion path so that once JSON is loaded into Project raw, it is immediately normalized:

var raw = LoadProjectJson(...);

var proj = ProjectNormalizer.Normalize(raw);

downstream takeoff calculations and rendering consume proj (native)

The normalized type must be used at least by the estimate pipeline. If other commands share the same Project load path, consider migrating them too only if low-risk.

5.2 Minimize signature churn

Prefer adapting internal calculation functions to accept:

TakeoffProject
or

explicit Length wallHeight, IReadOnlyList<Length> wallLengths, ...

Do not refactor public CLI surfaces for this ticket.

6. Tests (must add / update)
6.1 Normalization happy path

A minimal valid Project normalizes to TakeoffProject

Converted Length values match expected inches/feet

6.2 Negative / non-finite rejection

WallHeightFeet = -1 throws

WallLengthsFeet = [10, -5] throws

Penetration.WidthFeet = -1 throws

Any NaN/Infinity throws

6.3 WallIndex bounds

WallIndex = -1 throws

WallIndex = WallLengthsFeet.Length throws

6.4 Penetration extents

XFeet + WidthFeet > WallLengthFeet throws

YFeet + HeightFeet > WallHeightFeet throws

WidthFeet == 0 throws

HeightFeet == 0 throws

6.5 Regression guard (optional but valuable)

Load one existing examples/exampleproject*.json used by estimate and confirm normalization succeeds.

7. Definition of Done

pwsh -File scripts/preflight.ps1 passes (Release restore/build/test)

Estimate path uses TakeoffProject (native) after JSON load

New tests cover normalization + failure cases

No JSON schema changes required for existing projects

No raw feet doubles are used in core estimate calculations after normalization (acceptable for raw DTO only)