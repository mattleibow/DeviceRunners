using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DeviceRunners.Cli.Tests;

public class WindowsTestCommandTests
{
    private readonly CommandAppTester _app;

    public WindowsTestCommandTests()
    {
        _app = new CommandAppTester();
        _app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<WindowsTestCommand>("test");
            });
        });
    }

    [Fact]
    public void WithMissingApp_ShowsError()
    {
        // Act
        var result = _app.Run("windows", "test");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public void DefaultOutput_ContainsNoJson()
    {
        // Act - Run without --output flag
        var result = _app.Run("windows", "test");

        // Assert - Should contain verbose error messages, not JSON
        Assert.Contains("Application path is required", result.Output); // Error message should be verbose (required option missing)
        Assert.DoesNotContain("{", result.Output); // No JSON brackets
        Assert.DoesNotContain("\"success\"", result.Output); // No JSON properties
    }

    [Fact]
    public void JsonOutput_ContainsNoVerboseMessages()
    {
        // Act - Run with --output json flag
        var result = _app.Run("windows", "test", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(TestHelpers.IsValidJson(result.Output));
        // Error message text can appear in errorMessage field, but no verbose formatting or section headers
        Assert.DoesNotContain("Error:", result.Output); // No verbose "Error:" prefix
        Assert.DoesNotContain("PREPARATION", result.Output); // No verbose section headers
        Assert.Contains("\"success\"", result.Output); // Should have JSON structure
    }

    [Fact]
    public void WithMissingExeFile_ShowsFileNotFoundError()
    {
        // Arrange
        var nonExistentExe = "/tmp/non-existent-app.exe";

        // Act
        var result = _app.Run("windows", "test", "--app", nonExistentExe);

        // Assert
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Application file not found", result.Output);
    }

    [Fact]
    public void WithExeFile_GoesToUnpackagedPath()
    {
        // Arrange - Create a temporary exe file
        var tempExe = Path.GetTempFileName();
        File.Move(tempExe, tempExe + ".exe");
        tempExe += ".exe";

        try
        {
            // Act - Run with a very short timeout to avoid hanging
            var result = _app.Run("windows", "test", "--app", tempExe, "--connection-timeout", "1", "--data-timeout", "1");

            // Assert - Should show unpackaged app validation
            Assert.Contains("Validating unpackaged application", result.Output);
            Assert.Contains("Application validated", result.Output);
            Assert.Contains("Starting the unpackaged application", result.Output);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempExe))
                File.Delete(tempExe);
        }
    }

    [Fact]
    public void WithMsixFile_GoesToPackagedPath()
    {
        // Arrange
        var fakeMsix = "/tmp/fake-app.msix";

        // Act
        var result = _app.Run("windows", "test", "--app", fakeMsix);

        // Assert - Should attempt to process MSIX (and fail because it's not a real MSIX)
        Assert.NotEqual(0, result.ExitCode);
        // Should not show unpackaged validation message
        Assert.DoesNotContain("Validating unpackaged application", result.Output);
    }
}