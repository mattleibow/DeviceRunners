using DeviceRunners.Cli.Commands;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Text.Json;

namespace DeviceRunners.Cli.Tests;

public class CertificateCreateCommandTests
{
    [Fact]
    public void WithMissingParameters_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateCreateCommand>("install");
                });
            });
        });

        // Act
        var result = app.Run("windows", "cert", "install");

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
                windows.AddBranch("cert", cert =>
                {
                    cert.AddCommand<CertificateCreateCommand>("install");
                });
            });
        });

        // Act - Run without --output flag (should fail due to missing params but we check output format)
        var result = app.Run("windows", "cert", "install");

        // Assert - Should contain verbose messages, not JSON
        Assert.Contains("No parameters were provided", result.Output); // Error message should be verbose
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
                    cert.AddCommand<CertificateCreateCommand>("install");
                });
            });
        });

        // Act - Run with --output json flag
        var result = app.Run("windows", "cert", "install", "--output", "json");

        // Assert - Should contain clean JSON, no verbose messages
        Assert.True(IsValidJson(result.Output));
        Assert.DoesNotContain("PREPARATION", result.Output); // No verbose section headers
        Assert.DoesNotContain("Publisher identity", result.Output); // No verbose messages
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