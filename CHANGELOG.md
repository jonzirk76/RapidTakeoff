# Changelog

## v0.4.0
### Added
- Penetration-aware project model and validation (bounds checks, friendly errors, overlap warnings).
- Net-area math now uses gross minus merged penetration area.
- Elevation rendering for penetrations with opening framing visuals:
  - king studs, trimmers, headers, sills, and top/bottom cripples.
- Framed stud planning and category-aware stud totals (common, king, trimmer, cripple).
- New core framing planner tests covering door/window logic and regression behavior.

### Changed
- SVG output title updated from wall strips to elevations.
- Elevation layout now renders one larger wall per row for clearer detail.
- Assumptions are rendered in SVG below the summary block.
- Stud framing behavior at openings is now default (no feature flag).

## v0.3.0
### Added
- Wall strip output has been upgraded to full elevations by applying height rendering.  Dimension lines have been added to each elevation.

### Changed
- Renderer now includes logic that applies wall height to the SVG output.

## v0.2.0
### Added
- SVG output support for `rapid estimate` via `--format svg --out <path>`.
- New rendering module with wall-strip DTOs and an SVG renderer implementation.
- Rendering test project and SVG renderer tests to validate generated output.

### Changed
- README updated with SVG wall-strip usage examples.
- Solution and project wiring updated to include the rendering library and tests.

### Repository
- Default PR template now auto-loads via `.github/pull_request_template.md`.
- Generated `*.svg` outputs are ignored in `.gitignore`.

## v0.1.0
- Initial public release of RapidTakeoff CLI.
- Commands:
  - `drywall` — drywall sheet takeoff from wall lengths and height
  - `studs` — stud takeoff from wall lengths and spacing
  - `insulation` — insulation unit takeoff from wall lengths and height
  - `estimate` — project estimate from a JSON file (text or CSV output)
- Windows portable self-contained executable published via GitHub Releases.
