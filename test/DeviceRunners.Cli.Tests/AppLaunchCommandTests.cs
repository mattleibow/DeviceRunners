using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Text.Json;

namespace DeviceRunners.Cli.Tests;

public class AppLaunchCommandTests
{
    [Fact]
    public void AppLaunchCommand_DefaultOutput_ContainsNoJson()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<AppLaunchCommand>("launch");
            });
        });

        // Act - Run without --output flag
        var result = app.Run("windows", "launch");

        // Assert - Should contain verbose error messages, not JSON
        Assert.Contains("Either --app or --identity must be specified", result.Output); // Error message should be verbose
        Assert.DoesNotContain("{", result.Output); // No JSON brackets
        Assert.DoesNotContain("\"success\"", result.Output); // No JSON properties
    }

    [Fact]
    public void AppLaunchCommand_JsonOutput_ContainsNoVerboseMessages()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<AppLaunchCommand>("launch");
            });
        });

        // Act - Run with --output json flag
        var result = app.Run("windows", "launch", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(IsValidJson(result.Output));
        // Error message text can appear in errorMessage field, but no verbose formatting
        Assert.DoesNotContain("Error:", result.Output); // No verbose "Error:" prefix
        Assert.Contains("\"success\"", result.Output); // Should have JSON structure
    }

    private static bool IsValidJson(string jsonString)
    {
        try
        {
            JsonDocument.Parse(jsonString);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}