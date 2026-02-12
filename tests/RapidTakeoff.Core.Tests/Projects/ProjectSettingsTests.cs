using System.Text.Json;
using RapidTakeoff.Core.Projects;
using Xunit;

namespace RapidTakeoff.Core.Tests.Projects;

public sealed class ProjectSettingsTests
{
    [Fact]
    public void Ctor_Defaults_AreExpected()
    {
        var settings = new ProjectSettings();

        Assert.Equal("4x8", settings.DrywallSheet);
        Assert.Equal(0.10, settings.DrywallWaste, 10);
        Assert.Equal(16.0, settings.StudsSpacingInches, 10);
        Assert.Equal(0.0, settings.StudsWaste, 10);
        Assert.Equal(0.10, settings.InsulationWaste, 10);
        Assert.Equal(40.0, settings.InsulationCoverageSquareFeet, 10);
    }

    [Fact]
    public void Validate_ValidInput_DoesNotThrow()
    {
        var settings = new ProjectSettings
        {
            DrywallSheet = "4X12",
            DrywallWaste = 0.15,
            StudsSpacingInches = 24,
            StudsWaste = 0.05,
            InsulationWaste = 0.12,
            InsulationCoverageSquareFeet = 48
        };

        settings.Validate();

        Assert.Equal("4x12", settings.DrywallSheet);
    }

    [Fact]
    public void Validate_InvalidDrywallSheet_Throws()
    {
        var settings = new ProjectSettings { DrywallSheet = "5x10" };

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }

    [Fact]
    public void Validate_NegativeWaste_Throws()
    {
        var settings = new ProjectSettings { DrywallWaste = -0.01 };

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }

    [Fact]
    public void Validate_ZeroCoverage_Throws()
    {
        var settings = new ProjectSettings { InsulationCoverageSquareFeet = 0 };

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }

    [Fact]
    public void Json_RoundTrip_PreservesValues()
    {
        var original = new ProjectSettings
        {
            DrywallSheet = "4x12",
            DrywallWaste = 0.2,
            StudsSpacingInches = 24,
            StudsWaste = 0.05,
            InsulationWaste = 0.08,
            InsulationCoverageSquareFeet = 55
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ProjectSettings>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("4x12", deserialized.DrywallSheet);
        Assert.Equal(0.2, deserialized.DrywallWaste, 10);
        Assert.Equal(24, deserialized.StudsSpacingInches, 10);
        Assert.Equal(0.05, deserialized.StudsWaste, 10);
        Assert.Equal(0.08, deserialized.InsulationWaste, 10);
        Assert.Equal(55, deserialized.InsulationCoverageSquareFeet, 10);
    }
}
