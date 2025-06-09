using System.ComponentModel;
using System.Text.Json;
using System.Xml.Serialization;

using DeviceRunners.Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public abstract class BaseCommand<TSettings>(IAnsiConsole console) : Command<TSettings>
    where TSettings : BaseCommand<TSettings>.BaseCommandSettings
{
    public abstract class BaseCommandSettings : CommandSettings
    {
        [Description("Output format (json, xml, text)")]
        [CommandOption("--output")]
        public OutputFormat OutputFormat { get; set; } = OutputFormat.Default;
    }

    protected bool ShouldSuppressConsoleOutput(TSettings settings) =>
        settings.OutputFormat != OutputFormat.Default;

    protected void WriteResult<TResult>(TResult result, TSettings settings)
        where TResult : CommandResult
    {
        if (ShouldSuppressConsoleOutput(settings))
        {
            WriteOutput(result, settings.OutputFormat);
        }
    }

    protected void WriteConsoleOutput(string message, TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            console.MarkupLine(message);
        }
    }

    protected void WriteConsoleLine(TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            console.WriteLine();
        }
    }

    protected void WriteConsoleMarkup(string message, TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            console.MarkupLine(message);
        }
    }

    protected void WriteConsoleText(string text, TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            console.WriteLine(text);
        }
    }

    protected void WriteOutput<T>(T result, OutputFormat format)
        where T : CommandResult
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

    private void WriteJson<T>(T result)
        where T : CommandResult
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(result, options);
        console.WriteLine(json);
    }

    private void WriteXml<T>(T result)
        where T : CommandResult
    {
        var serializer = new XmlSerializer(typeof(T));
        using var writer = new StringWriter();
        serializer.Serialize(writer, result);
        console.WriteLine(writer.ToString());
    }

    private void WriteText<T>(T result)
        where T : CommandResult
    {
        // Simple key=value format
        console.WriteLine($"Success={result.Success}");
        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            console.WriteLine($"ErrorMessage={result.ErrorMessage}");
        }

        // Use reflection to write all properties
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != nameof(CommandResult.Success) && p.Name != nameof(CommandResult.ErrorMessage));

        foreach (var property in properties)
        {
            var value = property.GetValue(result);
            if (value != null)
            {
                console.WriteLine($"{property.Name}={value}");
            }
        }
    }
}
