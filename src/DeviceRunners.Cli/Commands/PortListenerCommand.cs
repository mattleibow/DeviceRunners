using System.ComponentModel;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class PortListenerCommand : Command<PortListenerCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("TCP port to listen on")]
        [CommandOption("--port")]
        [DefaultValue(16384)]
        public int Port { get; set; } = 16384;

        [Description("Path to save received data")]
        [CommandOption("--output")]
        public string? Output { get; set; }

        [Description("Run in non-interactive mode")]
        [CommandOption("--non-interactive")]
        public bool NonInteractive { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    private async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var networkService = new NetworkService();
            
            if (!networkService.IsPortAvailable(settings.Port))
            {
                AnsiConsole.MarkupLine($"[yellow]TCP Port {settings.Port} is already listening, aborting.[/]");
                return 1;
            }

            AnsiConsole.MarkupLine($"[green]TCP port {settings.Port} is available, continuing...[/]");
            AnsiConsole.MarkupLine($"[green]Now listening on TCP port {settings.Port}, press Ctrl+C to stop listening.[/]");
            
            if (settings.NonInteractive)
            {
                AnsiConsole.MarkupLine("[green]Listening in non-interactive mode, will terminate after first connection.[/]");
            }

            using var cancellationTokenSource = new CancellationTokenSource();
            
            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            var receivedData = await networkService.StartTcpListener(
                settings.Port, 
                settings.Output, 
                settings.NonInteractive, 
                cancellationTokenSource.Token);

            if (!string.IsNullOrEmpty(receivedData))
            {
                AnsiConsole.MarkupLine("[green]Data received:[/]");
                AnsiConsole.WriteLine(receivedData);
            }

            AnsiConsole.MarkupLine($"[green]Stopped listening on TCP port {settings.Port}[/]");
            return 0;
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine($"[yellow]Stopped listening on TCP port {settings.Port}[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}