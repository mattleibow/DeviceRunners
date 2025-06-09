using System.ComponentModel;

using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
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

    protected readonly OutputService outputService = new OutputService(console);

	protected bool ShouldSuppressConsoleOutput(TSettings settings) =>
        settings.OutputFormat != OutputFormat.Default;

	protected void WriteResult<TResult>(TResult result, TSettings settings)
        where TResult : CommandResult
    {
        if (ShouldSuppressConsoleOutput(settings))
        {
            outputService.WriteOutput(result, settings.OutputFormat);
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
}