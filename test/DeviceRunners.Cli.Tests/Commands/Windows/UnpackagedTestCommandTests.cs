using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DeviceRunners.Cli.Tests;

public class WindowsUnpackagedTestCommandTests
{
    private readonly CommandAppTester _app;

    public WindowsUnpackagedTestCommandTests()
    {
        _app = new CommandAppTester();
        _app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<WindowsUnpackagedTestCommand>("test-unpackaged");
            });
        });
    }

    [Fact]
    public void WithMissingApp_ShowsError()
    {
        // Act
        var result = _app.Run("windows", "test-unpackaged");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void WithNonExistentApp_ShowsError()
    {
        // Act
        var result = _app.Run("windows", "test-unpackaged", "--app", "nonexistent.exe");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Application file not found", result.Output);
    }

    [Fact]
    public void WithNonExeFile_ShowsError()
    {
        // Create a temporary non-exe file
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "test");
        
        try
        {
            // Act
            var result = _app.Run("windows", "test-unpackaged", "--app", tempFile);

            // Assert
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("Unpackaged apps must be .exe files", result.Output);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void DefaultOutput_ContainsNoJson()
    {
        // Act - Run without --output flag (this should fail because --app is required)
        var result = _app.Run("windows", "test-unpackaged");

        // Assert - Should contain verbose error messages, not JSON  
        // The test will fail because --app is required, so we get help text instead of our custom error
        Assert.True(result.Output.Length > 0); // Some output should be present
        Assert.DoesNotContain("{", result.Output); // No JSON brackets
        Assert.DoesNotContain("\"success\"", result.Output); // No JSON properties
    }

    [Fact]
    public void JsonOutput_ContainsNoVerboseMessages()
    {
        // Act - Run with --output json flag
        var result = _app.Run("windows", "test-unpackaged", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(TestHelpers.IsValidJson(result.Output));
        // Error message text can appear in errorMessage field, but no verbose formatting or section headers
        Assert.DoesNotContain("Error:", result.Output); // No verbose "Error:" prefix
        Assert.DoesNotContain("PREPARATION", result.Output); // No verbose section headers
        Assert.Contains("\"success\"", result.Output); // Should have JSON structure
    }
}