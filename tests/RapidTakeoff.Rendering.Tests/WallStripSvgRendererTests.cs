using RapidTakeoff.Rendering.WallStrips;

namespace RapidTakeoff.Rendering.Tests;

/// <summary>
/// Contains tests for <see cref="WallStripSvgRenderer"/>.
/// </summary>
public class WallStripSvgRendererTests
{
    /// <summary>
    /// Verifies that rendering a wall strip DTO returns an SVG document with expected structural markers.
    /// </summary>
    [Fact]
    public void Render_Should_Produce_Valid_Svg_Structure()
    {
        var dto = new WallStripDto(
            ProjectName: "Test Project",
            HeightFeet: 8,
            Walls: new[]
            {
                new WallSegmentDto("Wall 1", 12),
                new WallSegmentDto("Wall 2", 8)
            },
            Summary: new SummaryDto(
                TotalLengthFeet: 20,
                NetAreaSqFt: 160,
                DrywallSheets: 6,
                StudCount: 30,
                InsulationUnits: 160
            )
        );

        var renderer = new WallStripSvgRenderer();
        var svg = renderer.Render(dto);

        Assert.Contains("<svg", svg);
        Assert.Contains("RapidTakeoff — Wall Strips", svg);
        Assert.Contains("Project: Test Project", svg);

        Assert.Contains("<rect", svg);
        Assert.Contains("Wall 1", svg);
        Assert.Contains("8.00 ft H", svg);
        Assert.Contains("12.00 ft L", svg);
        Assert.Contains("Summary", svg);

        Assert.EndsWith("</svg>", svg.Trim());
    }
}
