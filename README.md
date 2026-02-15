# RapidTakeoff

Lightweight residential construction takeoff engine written in C#.

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

How to run examples:

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
  "settings": {
    "drywallSheet": "4x8",
    "drywallWaste": 0.10,
    "studsSpacingInches": 16,
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
| `settings` | `object` | No* | If omitted, defaults are used. If present, must be a valid object. |
| `settings.drywallSheet` | `string` | No | Allowed values: `4x8`, `4x12` (case-insensitive, surrounding whitespace ignored). |
| `settings.drywallWaste` | `number` | No | Finite number, `>= 0` (fraction form, e.g. `0.10` = 10%). |
| `settings.studsSpacingInches` | `number` | No | Finite number, `> 0`. |
| `settings.studsWaste` | `number` | No | Finite number, `>= 0` (fraction form). |
| `settings.insulationWaste` | `number` | No | Finite number, `>= 0` (fraction form). |
| `settings.insulationCoverageSquareFeet` | `number` | No | Finite number, `> 0`. |

`*` When `settings` is omitted, these defaults are applied:

- `drywallSheet`: `4x8`
- `drywallWaste`: `0.10`
- `studsSpacingInches`: `16`
- `studsWaste`: `0.0`
- `insulationWaste`: `0.10`
- `insulationCoverageSquareFeet`: `40`

Notes:

- JSON property names are case-insensitive.
- The estimate output format is selected separately via CLI `--format text`, `--format csv`, or `--format svg`.
- When using `--format svg`, `--out <path>` is required.

## SVG Wall Strip Output

Generate a scaled wall strip diagram (no room topology):

```bash
rapid estimate --project examples/project.basic.json --format svg --out walls.svg
```