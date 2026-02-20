# RapidTakeoff

Lightweight residential construction takeoff engine written in C#.

## What's New in v0.4.0
- Penetrations now drive net-area math and framed stud takeoff behavior.
- Elevations include opening framing visualization (kings, trimmers, headers, sills, cripples).
- Stud counts include framing categories, not just baseline spacing studs.
- Assumptions and framing behavior are documented in estimate output.

This repository focuses on:
- Provable, unit-tested takeoff math
- Clear unit and measurement handling
- A clean separation between core logic and UI

UI and distribution layers will be added later.


## Quickstart

### Option A: Download a release (Windows)
1. Download the latest `RapidTakeoff-win-x64.zip` from Releases.
2. Unzip it.
3. Run:

```powershell
.\rapid.exe --help
```
or run the .exe directly for interactive mode.

Copy/paste example:

```
.\rapid.exe estimate --project .\examples\exampleproject.json --format text
.\rapid.exe estimate --project .\examples\exampleproject.json --format csv
.\rapid.exe estimate --project .\examples\exampleproject.json --format svg --out .\out\estimate.svg
```

#### Option B: Run from source

```
.\scripts\preflight.ps1
dotnet run --project src\RapidTakeoff.Cli -- --help
```

## Project JSON Schema

Use this structure for `rapid estimate --project <file>`.

```json
{
  "name": "Basic Room Example",
  "wallHeightFeet": 8,
  "wallLengthsFeet": [12, 10, 12, 10],
  "penetrations": [
    {
      "id": "WIN-01",
      "type": "window",
      "wallIndex": 0,
      "xFeet": 3,
      "yFeet": 3,
      "widthFeet": 4,
      "heightFeet": 3
    }
  ],
  "settings": {
    "drywallSheet": "4x8",
    "drywallWaste": 0.10,
    "studsSpacingInches": 16,
    "studType": "2x4",
    "studsWaste": 0.05,
    "insulationWaste": 0.10,
    "insulationCoverageSquareFeet": 40
  }
}
```

Field requirements and accepted values:

| Field | Type | Required | Accepted values / rules |
|---|---|---|---|
| `name` | `string` | Yes | Non-empty, non-whitespace text. |
| `wallHeightFeet` | `number` | Yes | Finite number, `>= 0`. |
| `wallLengthsFeet` | `number[]` | Yes | At least 1 value. Each value must be finite and `>= 0`. |
| `penetrations` | `object[]` | No | Optional openings used in net wall area math. |
| `penetrations[].id` | `string` | No | Friendly identifier. If omitted/blank, a fallback label is used. |
| `penetrations[].type` | `string` | Yes* | Non-empty text such as `window` or `door`. |
| `penetrations[].wallIndex` | `number` | Yes* | Zero-based wall index; must be within `0..wallLengthsFeet.Length-1`. |
| `penetrations[].xFeet` | `number` | Yes* | Finite number, `>= 0`. |
| `penetrations[].yFeet` | `number` | Yes* | Finite number, `>= 0`. |
| `penetrations[].widthFeet` | `number` | Yes* | Finite number, `> 0`. |
| `penetrations[].heightFeet` | `number` | Yes* | Finite number, `> 0`. |
| `settings` | `object` | No* | If omitted, defaults are used. If present, must be a valid object. |
| `settings.drywallSheet` | `string` | No | Allowed values: `4x8`, `4x12` (case-insensitive, surrounding whitespace ignored). |
| `settings.drywallWaste` | `number` | No | Finite number, `>= 0` (fraction form, e.g. `0.10` = 10%). |
| `settings.studsSpacingInches` | `number` | No | Finite number, `> 0`. |
| `settings.studType` | `string` | No | Allowed values: `2x4`, `2x6`, `2x8`, `2x10`, `2x12` (case-insensitive, surrounding whitespace ignored). |
| `settings.studsWaste` | `number` | No | Finite number, `>= 0` (fraction form). |
| `settings.insulationWaste` | `number` | No | Finite number, `>= 0` (fraction form). |
| `settings.insulationCoverageSquareFeet` | `number` | No | Finite number, `> 0`. |

`*` For each entry in `penetrations`, these fields are required and validated.

`*` When `settings` is omitted, these defaults are applied:

- `drywallSheet`: `4x8`
- `drywallWaste`: `0.10`
- `studsSpacingInches`: `16`
- `studType`: `2x4`
- `studsWaste`: `0.0`
- `insulationWaste`: `0.10`
- `insulationCoverageSquareFeet`: `40`

Notes:

- JSON property names are case-insensitive.
- Penetration bounds are strictly validated against wall length/height.
- Overlapping penetrations produce warnings (not hard errors); net area uses merged opening area so overlap is not double-counted.
- The estimate output format is selected separately via CLI `--format text`, `--format csv`, or `--format svg`.
- When using `--format svg`, `--out <path>` is required.

## SVG Wall Elevation Output

Generate scaled wall elevations with length/height dimensions (no room topology):

```bash
rapid estimate --project examples/exampleproject2.json --format svg --out walls.svg
```
