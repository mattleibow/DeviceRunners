using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DeviceRunners.Cli.Tests;

public class WindowsAppLaunchCommandTests
{
    private readonly CommandAppTester _app;

    public WindowsAppLaunchCommandTests()
    {
        _app = new CommandAppTester();
        _app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<WindowsAppLaunchCommand>("launch");
            });
        });
    }

    [Fact]
    public void DefaultOutput_ContainsNoJson()
    {
        // Act - Run without --output flag
        var result = _app.Run("windows", "launch");

        // Assert - Should contain verbose error messages, not JSON
        Assert.Contains("Either --app or --identity must be specified", result.Output); // Error message should be verbose
        Assert.DoesNotContain("{", result.Output); // No JSON brackets
        Assert.DoesNotContain("\"success\"", result.Output); // No JSON properties
    }

    [Fact]
    public void JsonOutput_ContainsNoVerboseMessages()
    {
        // Act - Run with --output json flag
        var result = _app.Run("windows", "launch", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(TestHelpers.IsValidJson(result.Output));
        // Error message text can appear in errorMessage field, but no verbose formatting
        Assert.DoesNotContain("Error:", result.Output); // No verbose "Error:" prefix
        Assert.Contains("\"success\"", result.Output); // Should have JSON structure
    }
}