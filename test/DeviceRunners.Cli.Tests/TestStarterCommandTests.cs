using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Text.Json;

namespace DeviceRunners.Cli.Tests;

public class TestStarterCommandTests
{
    [Fact]
    public void WithMissingApp_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<TestStarterCommand>("test");
            });
        });

        // Act
        var result = app.Run("windows", "test");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void DefaultOutput_ContainsNoJson()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<TestStarterCommand>("test");
            });
        });

        // Act - Run without --output flag
        var result = app.Run("windows", "test");

        // Assert - Should contain verbose error messages, not JSON
        Assert.Contains("Certificate file not found", result.Output); // Error message should be verbose
        Assert.DoesNotContain("{", result.Output); // No JSON brackets
        Assert.DoesNotContain("\"success\"", result.Output); // No JSON properties
    }

    [Fact]
    public void JsonOutput_ContainsNoVerboseMessages()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<TestStarterCommand>("test");
            });
        });

        // Act - Run with --output json flag
        var result = app.Run("windows", "test", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(IsValidJson(result.Output));
        // Error message text can appear in errorMessage field, but no verbose formatting or section headers
        Assert.DoesNotContain("Error:", result.Output); // No verbose "Error:" prefix
        Assert.DoesNotContain("PREPARATION", result.Output); // No verbose section headers
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