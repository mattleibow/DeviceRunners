using System.ComponentModel;
using System.Text.Json;
using System.Xml.Serialization;

using DeviceRunners.Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public abstract class BaseAsyncCommand<TSettings>(IAnsiConsole console) : Command<TSettings>
    where TSettings : BaseAsyncCommand<TSettings>.BaseAsyncCommandSettings
{
    public abstract class BaseAsyncCommandSettings : CommandSettings
    {
        [Description("Output format (json, xml, text)")]
        [CommandOption("--output")]
        public OutputFormat OutputFormat { get; set; } = OutputFormat.Default;
    }

    public override int Execute(CommandContext context, TSettings settings)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    protected abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings);

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
            console.Markup(message);
        }
    }

    private void WriteOutput<TResult>(TResult result, OutputFormat format)
        where TResult : CommandResult
    {
        switch (format)
        {
            case OutputFormat.Json:
                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                console.WriteLine(json);
                break;

            case OutputFormat.Xml:
                var serializer = new XmlSerializer(typeof(TResult));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, result);
                    console.WriteLine(writer.ToString());
                }
                break;

            case OutputFormat.Text:
                WriteTextOutput(result);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    private void WriteTextOutput<TResult>(TResult result)
        where TResult : CommandResult
    {
        foreach (var prop in typeof(TResult).GetProperties())
        {
            var value = prop.GetValue(result);
            console.WriteLine($"{prop.Name}: {value}");
        }
    }
}
