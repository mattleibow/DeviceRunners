using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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

    public override int Execute(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    protected abstract Task<int> ExecuteAsync(CommandContext context, TSettings settings);

    protected bool ShouldSuppressConsoleOutput(TSettings settings) =>
        settings.OutputFormat != OutputFormat.Default;

    protected void WriteResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(TResult result, TSettings settings)
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

    private void WriteOutput<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(TResult result, OutputFormat format)
        where TResult : CommandResult
    {
        switch (format)
        {
            case OutputFormat.Json:
                var typeInfo = CliJsonContext.Default.GetTypeInfo(typeof(TResult)) as JsonTypeInfo<TResult>
                    ?? throw new InvalidOperationException($"Type '{typeof(TResult).Name}' is not registered in CliJsonContext. Add [JsonSerializable(typeof({typeof(TResult).Name}))] to the context.");
                var json = JsonSerializer.Serialize(result, typeInfo);
                console.WriteLine(json);
                break;

            case OutputFormat.Xml:
#pragma warning disable IL2026 // XML serialization is inherently reflection-based
                WriteXmlOutput(result);
#pragma warning restore IL2026
                break;

            case OutputFormat.Text:
                WriteTextOutput(result);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

#pragma warning disable IL2026 // XML serialization is inherently reflection-based; no source-gen alternative exists
    private void WriteXmlOutput<TResult>(TResult result)
        where TResult : CommandResult
    {
        var serializer = new XmlSerializer(typeof(TResult));
        using var writer = new StringWriter();
        serializer.Serialize(writer, result);
        console.WriteLine(writer.ToString());
    }
#pragma warning restore IL2026

    private void WriteTextOutput<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(TResult result)
        where TResult : CommandResult
    {
        foreach (var prop in typeof(TResult).GetProperties())
        {
            var value = prop.GetValue(result);
            console.WriteLine($"{prop.Name}: {value}");
        }
    }
}
