using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Text.Json;

namespace DeviceRunners.Cli.Tests;

public class CertificateRemoveCommandTests
{
    [Fact]
    public void OutputFormat_InvalidValue_HandledGracefully()
    {
        // Test that invalid output formats are handled by the CLI framework
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateRemoveCommand>("uninstall");
                });
            });
        });

        // Act - Try to use an invalid output format
        var result = app.Run("windows", "cert", "uninstall", "--fingerprint", "TEST123", "--output", "invalid");

        // Assert - Should fail with validation error
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("invalid", result.Output.ToLower());
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
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateRemoveCommand>("uninstall");
                });
            });
        });

        // Act - Run without --output flag
        var result = app.Run("windows", "cert", "uninstall");

        // Assert - Should contain verbose error messages, not JSON
        Assert.Contains("REMOVE CERTIFICATE", result.Output); // Verbose section headers
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
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateRemoveCommand>("uninstall");
                });
            });
        });

        // Act - Run with --output json flag
        var result = app.Run("windows", "cert", "uninstall", "--output", "json");

        // Assert - Should contain clean JSON error, no verbose messages
        Assert.True(IsValidJson(result.Output));
        Assert.DoesNotContain("REMOVE CERTIFICATE", result.Output); // No verbose section headers
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