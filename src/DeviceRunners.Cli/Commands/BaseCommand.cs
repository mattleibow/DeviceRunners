using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public abstract class BaseCommand<TSettings> : Command<TSettings> where TSettings : BaseCommandSettings
{
    protected readonly OutputService outputService = new();
    protected readonly IAnsiConsole console;

    protected BaseCommand(IAnsiConsole console)
    {
        this.console = console;
    }

    protected void WriteResult<TResult>(TResult result, TSettings settings) where TResult : CommandResult
    {
        if (settings.OutputFormat != OutputFormat.Default)
        {
            outputService.WriteOutput(result, settings.OutputFormat);
        }
    }

    protected bool ShouldSuppressConsoleOutput(TSettings settings)
    {
        return settings.OutputFormat != OutputFormat.Default;
    }

    protected void WriteConsoleOutput(string message, TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            console.MarkupLine(message);
        }
    }
}