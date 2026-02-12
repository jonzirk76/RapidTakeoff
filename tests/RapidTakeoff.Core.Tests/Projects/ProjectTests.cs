using System.Text.Json;
using RapidTakeoff.Core.Projects;
using Xunit;

namespace RapidTakeoff.Core.Tests.Projects;

public sealed class ProjectTests
{
    [Fact]
    public void Validate_ValidProject_DoesNotThrow()
    {
        var project = CreateValidProject();

        project.Validate();
    }

    [Fact]
    public void Validate_MissingName_Throws()
    {
        var project = CreateValidProject();
        project.Name = " ";

        Assert.Throws<ArgumentOutOfRangeException>(() => project.Validate());
    }

    [Fact]
    public void Validate_EmptyWallLengths_Throws()
    {
        var project = CreateValidProject();
        project.WallLengthsFeet = [];

        Assert.Throws<ArgumentOutOfRangeException>(() => project.Validate());
    }

    [Fact]
    public void Validate_NegativeWallHeight_Throws()
    {
        var project = CreateValidProject();
        project.WallHeightFeet = -1;

        Assert.Throws<ArgumentOutOfRangeException>(() => project.Validate());
    }

    [Fact]
    public void Validate_NegativeWallLength_Throws()
    {
        var project = CreateValidProject();
        project.WallLengthsFeet = [12, -10];

        Assert.Throws<ArgumentOutOfRangeException>(() => project.Validate());
    }

    [Fact]
    public void Validate_MissingSettings_Throws()
    {
        var project = CreateValidProject();
        project.Settings = null!;

        Assert.Throws<ArgumentNullException>(() => project.Validate());
    }

    [Fact]
    public void Json_RoundTrip_PreservesProject()
    {
        var original = CreateValidProject();

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Project>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("Garage Remodel", deserialized.Name);
        Assert.Equal(8.0, deserialized.WallHeightFeet, 10);
        Assert.Equal([12.0, 10.0, 12.0, 10.0], deserialized.WallLengthsFeet);
        Assert.NotNull(deserialized.Settings);
        Assert.Equal("4x8", deserialized.Settings.DrywallSheet);
    }

    private static Project CreateValidProject() =>
        new()
        {
            Name = "Garage Remodel",
            WallHeightFeet = 8.0,
            WallLengthsFeet = [12.0, 10.0, 12.0, 10.0],
            Settings = new ProjectSettings()
        };
}
