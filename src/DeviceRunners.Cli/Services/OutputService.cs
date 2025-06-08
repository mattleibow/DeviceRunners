using System.Text.Json;
using System.Xml.Serialization;
using DeviceRunners.Cli.Models;
using Spectre.Console;

namespace DeviceRunners.Cli.Services;

public class OutputService
{
    public void WriteOutput<T>(T result, OutputFormat format) where T : CommandResult
    {
        switch (format)
        {
            case OutputFormat.Json:
                WriteJson(result);
                break;
            case OutputFormat.Xml:
                WriteXml(result);
                break;
            case OutputFormat.Text:
                WriteText(result);
                break;
            case OutputFormat.Default:
                // Do nothing - normal console output is handled by the command
                break;
        }
    }

    private void WriteJson<T>(T result) where T : CommandResult
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(result, options);
        AnsiConsole.WriteLine(json);
    }

    private void WriteXml<T>(T result) where T : CommandResult
    {
        var serializer = new XmlSerializer(typeof(T));
        using var writer = new StringWriter();
        serializer.Serialize(writer, result);
        AnsiConsole.WriteLine(writer.ToString());
    }

    private void WriteText<T>(T result) where T : CommandResult
    {
        // Simple key=value format
        AnsiConsole.WriteLine($"Success={result.Success}");
        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            AnsiConsole.WriteLine($"ErrorMessage={result.ErrorMessage}");
        }

        // Use reflection to write all properties
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != nameof(CommandResult.Success) && p.Name != nameof(CommandResult.ErrorMessage));

        foreach (var property in properties)
        {
            var value = property.GetValue(result);
            if (value != null)
            {
                AnsiConsole.WriteLine($"{property.Name}={value}");
            }
        }
    }
}