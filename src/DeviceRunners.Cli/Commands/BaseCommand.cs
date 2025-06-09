using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public abstract class BaseCommand<TSettings> : Command<TSettings> where TSettings : BaseCommandSettings
{
    private readonly IAnsiConsole _console;
    protected readonly OutputService outputService;

    protected BaseCommand(IAnsiConsole console)
    {
        _console = console;
        outputService = new OutputService(console);
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
            _console.MarkupLine(message);
        }
    }

    protected void WriteConsoleLine(TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            _console.WriteLine();
        }
    }

    protected void WriteConsoleMarkup(string message, TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            _console.MarkupLine(message);
        }
    }

    protected void WriteConsoleText(string text, TSettings settings)
    {
        if (!ShouldSuppressConsoleOutput(settings))
        {
            _console.WriteLine(text);
        }
    }
}