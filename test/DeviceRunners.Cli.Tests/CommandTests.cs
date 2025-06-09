using DeviceRunners.Cli.Commands;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Text.Json;

namespace DeviceRunners.Cli.Tests;

public class CommandTests
{
    [Fact]
    public void CertificateCreateCommand_WithMissingParameters_ShowsError()
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
    public void TestStarterCommand_WithMissingApp_ShowsError()
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
    public void AppUninstallCommand_WithMissingParameters_ShowsError()
    {
        // Arrange
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.AddBranch("windows", windows =>
            {
                windows.AddCommand<AppUninstallCommand>("uninstall");
            });
        });

        // Act
        var result = app.Run("windows", "uninstall");

        // Assert
        Assert.NotEqual(0, result.ExitCode);
    }

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
    public void OutputService_ProducesValidJsonOutput()
    {
        // Arrange
        var testResult = new CertificateRemoveResult
        {
            Success = true,
            Fingerprint = "ABC123",
            WasFound = true
        };
        var console = new TestConsole();
        var outputService = new OutputService(console);

        // Act
        outputService.WriteOutput(testResult, OutputFormat.Json);
        var output = console.Output;

        // Assert
        Assert.True(IsValidJson(output));
        var jsonDoc = JsonDocument.Parse(output);
        Assert.True(jsonDoc.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("ABC123", jsonDoc.RootElement.GetProperty("fingerprint").GetString());
        Assert.True(jsonDoc.RootElement.GetProperty("wasFound").GetBoolean());
    }

    [Fact]
    public void OutputService_ProducesValidXmlOutput()
    {
        // Arrange
        var testResult = new CertificateRemoveResult
        {
            Success = true,
            Fingerprint = "ABC123",
            WasFound = true
        };
        var console = new TestConsole();
        var outputService = new OutputService(console);

        // Act
        outputService.WriteOutput(testResult, OutputFormat.Xml);
        var output = console.Output;

        // Assert
        Assert.Contains("<?xml", output);
        Assert.Contains("<CertificateRemoveResult", output);
        Assert.Contains("<Success>true</Success>", output);
        Assert.Contains("<Fingerprint>ABC123</Fingerprint>", output);
        
        // Verify XML is parseable
        var xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.LoadXml(output);
        Assert.NotNull(xmlDoc.DocumentElement);
    }

    [Fact]
    public void OutputService_ProducesValidTextOutput()
    {
        // Arrange
        var testResult = new CertificateRemoveResult
        {
            Success = true,
            Fingerprint = "ABC123",
            WasFound = true
        };
        var console = new TestConsole();
        var outputService = new OutputService(console);

        // Act
        outputService.WriteOutput(testResult, OutputFormat.Text);
        var output = console.Output;

        // Assert
        Assert.Contains("Success=True", output);
        Assert.Contains("Fingerprint=ABC123", output);
        Assert.Contains("WasFound=True", output);
        
        // Verify text format doesn't contain JSON or XML markers
        Assert.DoesNotContain("{", output);
        Assert.DoesNotContain("<?xml", output);
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