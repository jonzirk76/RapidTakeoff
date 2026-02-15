using System.Text;

namespace RapidTakeoff.Rendering.WallStrips;

/// <summary>
/// Renders a simple, deterministic SVG "wall strip" diagram for independent wall entities.
/// This renderer is intentionally dumb: it consumes a render DTO and emits SVG markup.
/// </summary>
public sealed class WallStripSvgRenderer
{
    /// <summary>
    /// Generates an SVG document as a string.
    /// </summary>
    /// <param name="dto">Render-ready wall strip data.</param>
    /// <returns>SVG document text.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public string Render(WallStripDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        if (dto.Walls is null) throw new ArgumentException("Walls collection is null.", nameof(dto));
        if (dto.Walls.Count == 0) throw new ArgumentException("Walls collection is empty.", nameof(dto));

        // v0 skeleton: fixed canvas, title + project. We'll add layout next process.
        const int width = 1000;
        const int height = 700;

        var sb = new StringBuilder(capacity: 4096);

        sb.AppendLine($@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{width}"" height=""{height}"" viewBox=""0 0 {width} {height}"">");

        // Title block
        sb.AppendLine(@"  <text x=""50"" y=""40"" font-family=""Arial"" font-size=""24"">RapidTakeoff — Wall Strips</text>");
        sb.AppendLine($@"  <text x=""50"" y=""70"" font-family=""Arial"" font-size=""14"">Project: {EscapeXml(dto.ProjectName)}</text>");

        const int leftMargin = 150;
        const int rightMargin = 150;
        const int topStart = 120;
        const int rowSpacing = 50;
        const int stripHeight = 12;
        const int drawableWidth = width - leftMargin - rightMargin;

        var longestWall = dto.Walls.Max(w => w.LengthFeet);
        var pixelsPerFoot = longestWall > 0
            ? drawableWidth / longestWall
            : 1;

        for (int i = 0; i < dto.Walls.Count; i++)
        {
            var wall = dto.Walls[i];

            var y = topStart + i * rowSpacing;
            var rectWidth = wall.LengthFeet * pixelsPerFoot;

            // Wall label
            sb.AppendLine(
                $@"  <text x=""50"" y=""{y + stripHeight}"" font-family=""Arial"" font-size=""14"">{EscapeXml(wall.Name)}</text>");

            // Wall strip rectangle
            sb.AppendLine(
                $@"  <rect x=""{leftMargin}"" y=""{y}"" width=""{rectWidth:F2}"" height=""{stripHeight}"" fill=""none"" stroke=""black"" stroke-width=""1"" />");

            // Length label
            sb.AppendLine(
                $@"  <text x=""{leftMargin + rectWidth + 10:F2}"" y=""{y + stripHeight}"" font-family=""Arial"" font-size=""12"">{wall.LengthFeet:F2} ft</text>");
        }

        var summaryStartY = topStart + dto.Walls.Count * rowSpacing + 60;

        sb.AppendLine($@"  <text x=""50"" y=""{summaryStartY}"" font-family=""Arial"" font-size=""16"">Summary</text>");
        summaryStartY += 25;

        sb.AppendLine($@"  <text x=""50"" y=""{summaryStartY}"" font-size=""12"">Total Length: {dto.Summary.TotalLengthFeet:F2} ft</text>");
        summaryStartY += 20;

        sb.AppendLine($@"  <text x=""50"" y=""{summaryStartY}"" font-size=""12"">Height: {dto.HeightFeet:F2} ft</text>");
        summaryStartY += 20;

        sb.AppendLine($@"  <text x=""50"" y=""{summaryStartY}"" font-size=""12"">Net Area: {dto.Summary.NetAreaSqFt:F2} sq ft</text>");
        summaryStartY += 20;

        sb.AppendLine($@"  <text x=""50"" y=""{summaryStartY}"" font-size=""12"">Drywall Sheets: {dto.Summary.DrywallSheets}</text>");
        summaryStartY += 20;

        sb.AppendLine($@"  <text x=""50"" y=""{summaryStartY}"" font-size=""12"">Stud Count: {dto.Summary.StudCount}</text>");
        summaryStartY += 20;

        sb.AppendLine($@"  <text x=""50"" y=""{summaryStartY}"" font-size=""12"">Insulation Units: {dto.Summary.InsulationUnits}</text>");

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    /// <summary>
    /// Escapes text for safe inclusion in SVG/XML text nodes and attributes.
    /// </summary>
    private static string EscapeXml(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Minimal XML escaping (sufficient for SVG text/attrs)
        return text.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("\"", "&quot;")
                   .Replace("'", "&apos;");
    }
}
