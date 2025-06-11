using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DeviceRunners.Cli.Tests;

public class WindowCertificateRemoveCommandTests
{
    private readonly CommandAppTester _app;

    public WindowCertificateRemoveCommandTests()
    {
        _app = new CommandAppTester();
        _app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<WindowCertificateRemoveCommand>("uninstall");
                });
            });
        });
    }

    [Fact]
    public void OutputFormat_InvalidValue_HandledGracefully()
    {
        // Act - Try to use an invalid output format
        var result = _app.Run("windows", "cert", "uninstall", "--fingerprint", "TEST123", "--output", "invalid");

        // Assert - Should fail with validation error
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("invalid", result.Output.ToLower());
    }

    [Fact]
    public void DefaultOutput_ContainsNoJson()
    {
        // Act - Run without --output flag
        var result = _app.Run("windows", "cert", "uninstall");

        // Assert - Should contain verbose error messages, not JSON
        Assert.Contains("REMOVE CERTIFICATE", result.Output); // Verbose section headers
        Assert.DoesNotContain("{", result.Output); // No JSON brackets
        Assert.DoesNotContain("\"success\"", result.Output); // No JSON properties
    }

    [Fact]
    public void JsonOutput_ContainsNoVerboseMessages()
    {
        // Act - Run with --output json flag
        var result = _app.Run("windows", "cert", "uninstall", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(TestHelpers.IsValidJson(result.Output));
        Assert.DoesNotContain("REMOVE CERTIFICATE", result.Output); // No verbose section headers
        Assert.Contains("\"success\"", result.Output); // Should have JSON structure
    }
}