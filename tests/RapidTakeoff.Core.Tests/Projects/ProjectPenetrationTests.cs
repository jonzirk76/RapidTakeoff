using RapidTakeoff.Core.Projects;
using Xunit;

namespace RapidTakeoff.Core.Tests.Projects;

public sealed class ProjectPenetrationTests
{
    [Fact]
    public void Validate_Penetration_OutsideWallLength_ThrowsFriendlyMessage()
    {
        var project = CreateBaseProject();
        project.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "WIN-01",
                Type = "window",
                WallIndex = 0,
                XFeet = 10.5,
                YFeet = 2.0,
                WidthFeet = 2.0,
                HeightFeet = 3.0
            }
        ];

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => project.Validate());
        Assert.Contains("exceeds wall 1 length", ex.Message);
        Assert.Contains("WIN-01", ex.Message);
    }

    [Fact]
    public void Validate_Penetration_OutsideWallHeight_ThrowsFriendlyMessage()
    {
        var project = CreateBaseProject();
        project.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "DR-01",
                Type = "door",
                WallIndex = 1,
                XFeet = 1.0,
                YFeet = 6.0,
                WidthFeet = 3.0,
                HeightFeet = 2.5
            }
        ];

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => project.Validate());
        Assert.Contains("exceeds wall height", ex.Message);
        Assert.Contains("DR-01", ex.Message);
    }

    [Fact]
    public void GetValidationWarnings_Overlaps_ReturnsWarning()
    {
        var project = CreateBaseProject();
        project.Penetrations =
        [
            new ProjectPenetration { Id = "WIN-01", Type = "window", WallIndex = 0, XFeet = 2, YFeet = 2, WidthFeet = 4, HeightFeet = 3 },
            new ProjectPenetration { Id = "WIN-02", Type = "window", WallIndex = 0, XFeet = 4, YFeet = 3, WidthFeet = 4, HeightFeet = 3 }
        ];

        project.Validate();
        var warnings = project.GetValidationWarnings();

        Assert.Single(warnings);
        Assert.Contains("overlap on wall 1", warnings[0]);
        Assert.Contains("WIN-01", warnings[0]);
        Assert.Contains("WIN-02", warnings[0]);
    }

    [Fact]
    public void GetPenetrationAreaSquareFeet_OverlappingOpenings_MergesArea()
    {
        var project = CreateBaseProject();
        project.Penetrations =
        [
            new ProjectPenetration { Id = "A", Type = "window", WallIndex = 0, XFeet = 2, YFeet = 2, WidthFeet = 4, HeightFeet = 3 },
            new ProjectPenetration { Id = "B", Type = "window", WallIndex = 0, XFeet = 4, YFeet = 3, WidthFeet = 4, HeightFeet = 3 }
        ];

        project.Validate();

        // Individual areas: 12 + 12 = 24 sqft. Overlap is 2x2 = 4 sqft, so merged area is 20 sqft.
        var areaSqFt = project.GetPenetrationAreaSquareFeet();
        Assert.Equal(20.0, areaSqFt, 6);
    }

    [Fact]
    public void GetNetWallAreaSquareFeet_SubtractsPenetrationsFromGrossArea()
    {
        var project = CreateBaseProject();
        project.Penetrations =
        [
            new ProjectPenetration { Id = "DR-01", Type = "door", WallIndex = 0, XFeet = 1.0, YFeet = 0.0, WidthFeet = 3.0, HeightFeet = 6.8 },
            new ProjectPenetration { Id = "WIN-01", Type = "window", WallIndex = 1, XFeet = 3.0, YFeet = 3.0, WidthFeet = 4.0, HeightFeet = 3.0 }
        ];

        project.Validate();

        var grossSqFt = project.GetGrossWallAreaSquareFeet();
        var penetrationSqFt = project.GetPenetrationAreaSquareFeet();
        var netSqFt = project.GetNetWallAreaSquareFeet();

        Assert.Equal(352.0, grossSqFt, 6); // (12+10+12+10) * 8
        Assert.Equal(32.4, penetrationSqFt, 6); // (3*6.8) + (4*3)
        Assert.Equal(319.6, netSqFt, 6);
    }

    private static Project CreateBaseProject() =>
        new()
        {
            Name = "Penetration Math Test",
            WallHeightFeet = 8.0,
            WallLengthsFeet = [12.0, 10.0, 12.0, 10.0],
            Settings = new ProjectSettings()
        };
}
