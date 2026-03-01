using System.Text.Json;
using RapidTakeoff.Core.Projects;

namespace RapidTakeoff.Core.Tests.Projects;

public sealed class ProjectNormalizerTests
{
    [Fact]
    public void Normalize_MinimalValidProject_ReturnsTakeoffProjectWithNativeLengths()
    {
        var raw = CreateValidProject();

        var normalized = ProjectNormalizer.Normalize(raw);

        Assert.Equal(raw.Name, normalized.Name);
        Assert.Equal(8.0, normalized.WallHeight.TotalFeet, 10);
        Assert.Equal([12.0, 10.0, 12.0, 10.0], normalized.WallLengths.Select(x => x.TotalFeet).ToArray());
        Assert.NotNull(normalized.Settings);
        Assert.Empty(normalized.Penetrations);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Normalize_NonFiniteWallHeight_Throws(double value)
    {
        var raw = CreateValidProject();
        raw.WallHeightFeet = value;

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_NegativeWallHeight_Throws()
    {
        var raw = CreateValidProject();
        raw.WallHeightFeet = -1.0;

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_NegativeWallLength_Throws()
    {
        var raw = CreateValidProject();
        raw.WallLengthsFeet = [10.0, -5.0];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Normalize_NonFiniteWallLength_Throws(double value)
    {
        var raw = CreateValidProject();
        raw.WallLengthsFeet = [10.0, value];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Normalize_NonFinitePenetrationWidth_Throws(double value)
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 1.0,
                YFeet = 1.0,
                WidthFeet = value,
                HeightFeet = 2.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_NegativePenetrationWidth_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 1.0,
                YFeet = 1.0,
                WidthFeet = -1.0,
                HeightFeet = 2.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_WallIndexNegative_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "door",
                WallIndex = -1,
                XFeet = 0.0,
                YFeet = 0.0,
                WidthFeet = 3.0,
                HeightFeet = 7.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_WallIndexOutOfRange_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "door",
                WallIndex = raw.WallLengthsFeet.Length,
                XFeet = 0.0,
                YFeet = 0.0,
                WidthFeet = 3.0,
                HeightFeet = 7.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_PenetrationXPlusWidthExceedsWall_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 11.0,
                YFeet = 2.0,
                WidthFeet = 2.0,
                HeightFeet = 2.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_PenetrationXExceedsWall_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 13.0,
                YFeet = 1.0,
                WidthFeet = 1.0,
                HeightFeet = 1.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_PenetrationYPlusHeightExceedsWallHeight_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 2.0,
                YFeet = 7.0,
                WidthFeet = 2.0,
                HeightFeet = 2.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_PenetrationYExceedsWallHeight_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 1.0,
                YFeet = 9.0,
                WidthFeet = 1.0,
                HeightFeet = 1.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_ZeroPenetrationWidth_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 1.0,
                YFeet = 1.0,
                WidthFeet = 0.0,
                HeightFeet = 2.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_ZeroPenetrationHeight_Throws()
    {
        var raw = CreateValidProject();
        raw.Penetrations =
        [
            new ProjectPenetration
            {
                Id = "P1",
                Type = "window",
                WallIndex = 0,
                XFeet = 1.0,
                YFeet = 1.0,
                WidthFeet = 2.0,
                HeightFeet = 0.0
            }
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectNormalizer.Normalize(raw));
    }

    [Fact]
    public void Normalize_ExampleProjectJson_Succeeds()
    {
        var examplePath = FindExamplePath("exampleproject.json");
        var json = File.ReadAllText(examplePath);
        var raw = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(raw);
        var normalized = ProjectNormalizer.Normalize(raw!);
        Assert.NotEmpty(normalized.WallLengths);
    }

    private static Project CreateValidProject() =>
        new()
        {
            Name = "Garage Remodel",
            WallHeightFeet = 8.0,
            WallLengthsFeet = [12.0, 10.0, 12.0, 10.0],
            Settings = new ProjectSettings(),
            Penetrations = []
        };

    private static string FindExamplePath(string fileName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "examples", fileName);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException($"Could not locate examples/{fileName} from test base directory.");
    }
}
