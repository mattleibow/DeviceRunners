using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Testing;
using System.Text.Json;

namespace DeviceRunners.Cli.Tests;

public class OutputServiceTests
{
    [Fact]
    public void ProducesValidJsonOutput()
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
    public void ProducesValidXmlOutput()
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
    public void ProducesValidTextOutput()
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