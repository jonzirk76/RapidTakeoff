# Changelog

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
