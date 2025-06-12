using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DeviceRunners.Cli.Tests;

public class MacOSTestCommandTests
{
    private readonly CommandAppTester _app;

    public MacOSTestCommandTests()
    {
        _app = new CommandAppTester();
        _app.Configure(config =>
        {
            config.AddBranch("macos", macos =>
            {
                macos.AddCommand<MacOSTestCommand>("test");
            });
        });
    }

    [Fact]
    public void WithMissingApp_ShowsError()
    {
        // Act
        var result = _app.Run("macos", "test");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void DefaultOutput_ContainsNoJson()
    {
        // Act - Run without --output flag
        var result = _app.Run("macos", "test");

        // Assert - Should fail and not contain JSON
        Assert.NotEqual(0, result.ExitCode);
        Assert.DoesNotContain("{", result.Output); // No JSON brackets
        Assert.DoesNotContain("\"success\"", result.Output); // No JSON properties
    }

    [Fact]
    public void JsonOutput_ContainsNoVerboseMessages()
    {
        // Act - Run with --output json flag
        var result = _app.Run("macos", "test", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(TestHelpers.IsValidJson(result.Output));
        // Error message text can appear in errorMessage field, but no verbose formatting or section headers
        Assert.DoesNotContain("Error:", result.Output); // No verbose "Error:" prefix
        Assert.DoesNotContain("PREPARATION", result.Output); // No verbose section headers
        Assert.Contains("\"success\"", result.Output); // Should have JSON structure
    }
}