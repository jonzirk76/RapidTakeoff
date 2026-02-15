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
.\rapid.exe estimate --project .\examples\project.basic.json --format text
.\rapid.exe estimate --project .\examples\project.basic.json --format csv
```

#### Option B: Run from source

```
.\scripts\preflight.ps1
dotnet run --project src\RapidTakeoff.Cli -- --help
```