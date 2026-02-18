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

        const string svgBackgroundColor = "#f8fafc";
        const string wallFillColor = "#c7d9eb";

        const int width = 1200;
        const int leftMargin = 60;
        const int rightMargin = 60;
        const int topStart = 120;
        const int footerPadding = 200;
        const int maxElevationHeight = 120;
        const double minPixelsPerFoot = 2.0;
        const int wallGapX = 56;
        const int rowGapY = 36;
        const int rightDimReserve = 95;
        const int drawableWidth = width - leftMargin - rightMargin;

        var safeHeightFeet = Math.Max(0, dto.HeightFeet);
        var totalLengthFeet = Math.Max(1, dto.Walls.Sum(w => w.LengthFeet));
        var totalGapWidth = wallGapX * Math.Max(0, dto.Walls.Count - 1);
        var totalReserveWidth = rightDimReserve * dto.Walls.Count;
        var widthPixelsPerFoot = (drawableWidth - totalGapWidth - totalReserveWidth) / totalLengthFeet;
        var heightPixelsPerFoot = safeHeightFeet > 0
            ? maxElevationHeight / safeHeightFeet
            : widthPixelsPerFoot;
        var pixelsPerFoot = Math.Max(minPixelsPerFoot, Math.Min(widthPixelsPerFoot, heightPixelsPerFoot));

        // Keep one feet-to-pixels scale for both axes so wall proportions are realistic.
        var stripHeight = Math.Max(1, (int)Math.Round(safeHeightFeet * pixelsPerFoot));
        var wallBlockHeight = stripHeight + 62;

        var xCursor = leftMargin;
        var yCursor = topStart;
        var wallBottom = topStart;

        foreach (var wall in dto.Walls)
        {
            var rectWidth = wall.LengthFeet * pixelsPerFoot;
            var blockWidth = rectWidth + rightDimReserve;
            if (xCursor > leftMargin && xCursor + blockWidth > width - rightMargin)
            {
                xCursor = leftMargin;
                yCursor += wallBlockHeight + rowGapY;
            }

            wallBottom = Math.Max(wallBottom, yCursor + wallBlockHeight);
            xCursor += (int)Math.Ceiling(blockWidth + wallGapX);
        }

        var summaryStartY = wallBottom + 50;
        var height = Math.Max(700, summaryStartY + footerPadding);

        var sb = new StringBuilder(capacity: 6144);

        sb.AppendLine($@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{width}"" height=""{height}"" viewBox=""0 0 {width} {height}"">");
        sb.AppendLine($@"  <rect x=""0"" y=""0"" width=""{width}"" height=""{height}"" fill=""{svgBackgroundColor}"" />");

        // Title block
        sb.AppendLine(@"  <text x=""50"" y=""40"" font-family=""Arial"" font-size=""24"">RapidTakeoff — Wall Strips</text>");
        sb.AppendLine($@"  <text x=""50"" y=""70"" font-family=""Arial"" font-size=""14"">Project: {EscapeXml(dto.ProjectName)}</text>");

        xCursor = leftMargin;
        yCursor = topStart;

        for (int i = 0; i < dto.Walls.Count; i++)
        {
            var wall = dto.Walls[i];

            var rectWidth = wall.LengthFeet * pixelsPerFoot;
            var blockWidth = rectWidth + rightDimReserve;
            if (xCursor > leftMargin && xCursor + blockWidth > width - rightMargin)
            {
                xCursor = leftMargin;
                yCursor += wallBlockHeight + rowGapY;
            }

            // Wall rectangle scaled by length and common project height.
            var rectRight = xCursor + rectWidth;
            var rectBottom = yCursor + stripHeight;
            sb.AppendLine(
                $@"  <rect x=""{xCursor}"" y=""{yCursor}"" width=""{rectWidth:F2}"" height=""{stripHeight}"" fill=""{wallFillColor}"" stroke=""#1f2937"" stroke-width=""1"" />");

            // Penetrations rendered as white cutouts in wall-local coordinates.
            foreach (var penetration in wall.Penetrations ?? [])
            {
                if (penetration.WidthFeet <= 0 || penetration.HeightFeet <= 0)
                    continue;

                var penetrationX = xCursor + (penetration.XFeet * pixelsPerFoot);
                var penetrationWidth = penetration.WidthFeet * pixelsPerFoot;
                var penetrationY = yCursor + stripHeight - ((penetration.YFeet + penetration.HeightFeet) * pixelsPerFoot);
                var penetrationHeight = penetration.HeightFeet * pixelsPerFoot;

                var clippedLeft = Math.Max(xCursor, penetrationX);
                var clippedRight = Math.Min(rectRight, penetrationX + penetrationWidth);
                var clippedTop = Math.Max(yCursor, penetrationY);
                var clippedBottom = Math.Min(rectBottom, penetrationY + penetrationHeight);

                if (clippedRight <= clippedLeft || clippedBottom <= clippedTop)
                    continue;

                var clippedWidth = clippedRight - clippedLeft;
                var clippedHeight = clippedBottom - clippedTop;

                sb.AppendLine(
                    $@"  <rect class=""penetration"" x=""{clippedLeft:F2}"" y=""{clippedTop:F2}"" width=""{clippedWidth:F2}"" height=""{clippedHeight:F2}"" fill=""{svgBackgroundColor}"" stroke=""#0f172a"" stroke-width=""1"" stroke-dasharray=""4 2"" />");

                if (clippedWidth >= 38 && clippedHeight >= 16)
                {
                    sb.AppendLine(
                        $@"  <text x=""{clippedLeft + (clippedWidth / 2):F2}"" y=""{clippedTop + (clippedHeight / 2) + 4:F2}"" font-family=""Arial"" font-size=""10"" text-anchor=""middle"">{EscapeXml(penetration.Id)}</text>");
                }
            }

            // Height dimension marker and label.
            var dimX = rectRight + 22;
            sb.AppendLine($@"  <line x1=""{dimX}"" y1=""{yCursor}"" x2=""{dimX}"" y2=""{rectBottom}"" stroke=""#374151"" stroke-width=""1"" />");
            sb.AppendLine($@"  <line x1=""{dimX - 5}"" y1=""{yCursor}"" x2=""{dimX + 5}"" y2=""{yCursor}"" stroke=""#374151"" stroke-width=""1"" />");
            sb.AppendLine($@"  <line x1=""{dimX - 5}"" y1=""{rectBottom}"" x2=""{dimX + 5}"" y2=""{rectBottom}"" stroke=""#374151"" stroke-width=""1"" />");
            sb.AppendLine(
                $@"  <text x=""{dimX + 10}"" y=""{yCursor + (stripHeight / 2) + 4}"" font-family=""Arial"" font-size=""11"" text-anchor=""start"">{dto.HeightFeet:F2} ft H</text>");

            // Length dimension marker and label along the bottom side.
            var dimY = rectBottom + 18;
            sb.AppendLine($@"  <line x1=""{xCursor}"" y1=""{dimY}"" x2=""{rectRight:F2}"" y2=""{dimY}"" stroke=""#374151"" stroke-width=""1"" />");
            sb.AppendLine($@"  <line x1=""{xCursor}"" y1=""{dimY - 5}"" x2=""{xCursor}"" y2=""{dimY + 5}"" stroke=""#374151"" stroke-width=""1"" />");
            sb.AppendLine($@"  <line x1=""{rectRight:F2}"" y1=""{dimY - 5}"" x2=""{rectRight:F2}"" y2=""{dimY + 5}"" stroke=""#374151"" stroke-width=""1"" />");
            sb.AppendLine(
                $@"  <text x=""{xCursor + (rectWidth / 2):F2}"" y=""{dimY + 16}"" font-family=""Arial"" font-size=""12"" text-anchor=""middle"">{wall.LengthFeet:F2} ft L</text>");

            // Wall title under each elevation.
            sb.AppendLine(
                $@"  <text x=""{xCursor + (rectWidth / 2):F2}"" y=""{dimY + 38}"" font-family=""Arial"" font-size=""14"" text-anchor=""middle"">{EscapeXml(wall.Name)}</text>");

            xCursor += (int)Math.Ceiling(blockWidth + wallGapX);
        }

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
