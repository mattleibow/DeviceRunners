using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class PortListenerCommand(IAnsiConsole console) : BaseCommand<PortListenerCommand.Settings>(console)
{
	public class Settings : BaseCommandSettings
    {
        [Description("TCP port to listen on")]
        [CommandOption("--port")]
        [DefaultValue(16384)]
        public int Port { get; set; } = 16384;

        [Description("Path to save received data")]
        [CommandOption("--results-file")]
        public string? ResultsFile { get; set; }

        [Description("Run in non-interactive mode")]
        [CommandOption("--non-interactive")]
        public bool NonInteractive { get; set; }

        [Description("Connection timeout in seconds (non-interactive mode only)")]
        [CommandOption("--connection-timeout")]
        public int? ConnectionTimeout { get; set; }

        [Description("Data timeout in seconds (non-interactive mode only)")]
        [CommandOption("--data-timeout")]
        public int? DataTimeout { get; set; }
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
            
            // Subscribe to network events for real-time logging
            networkService.ConnectionEstablished += (sender, e) =>
            {
                var endpointInfo = e.RemoteEndPoint?.ToString() ?? "unknown";
                WriteConsoleOutput($"[green]Incoming connection from {endpointInfo}, responding...[/]", settings);
            };
            
            networkService.ConnectionClosed += (sender, e) =>
            {
                var endpointInfo = e.RemoteEndPoint?.ToString() ?? "unknown";
                WriteConsoleOutput($"[green]Connection from {endpointInfo} closed[/]", settings);
            };
            
            networkService.DataReceived += (sender, e) =>
            {
                WriteConsoleOutput($"[green]Data received:[/] {Markup.Escape(e.Data)}", settings);
            };
            
            if (!networkService.IsPortAvailable(settings.Port))
            {
                var result = new PortListenerResult
                {
                    Success = false,
                    ErrorMessage = $"TCP Port {settings.Port} is already listening",
                    Port = settings.Port
                };

                WriteConsoleOutput($"[yellow]TCP Port {settings.Port} is already listening, aborting.[/]", settings);
                WriteResult(result, settings);
                return 1;
            }

            WriteConsoleOutput($"[green]TCP port {settings.Port} is available, continuing...[/]", settings);
            WriteConsoleOutput($"[green]Now listening on TCP port {settings.Port}, press Ctrl+C to stop listening.[/]", settings);

            if (settings.NonInteractive)
            {
                WriteConsoleOutput($"[green]Listening in non-interactive mode, will terminate after first connection.[/]", settings);

                if (settings.ConnectionTimeout.HasValue)
                {
                    WriteConsoleOutput($"[green]Connection timeout set to {settings.ConnectionTimeout} seconds.[/]", settings);
                }
                if (settings.DataTimeout.HasValue)
                {
                    WriteConsoleOutput($"[green]Data timeout set to {settings.DataTimeout} seconds.[/]", settings);
                }
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
                settings.ResultsFile, 
                settings.NonInteractive, 
                settings.ConnectionTimeout,
                settings.DataTimeout,
                cancellationTokenSource.Token);

            if (!string.IsNullOrEmpty(receivedData))
            {
                WriteConsoleOutput($"[green]Data received:[/]", settings);
                WriteConsoleText(receivedData, settings);
            }

            WriteConsoleOutput($"[green]Stopped listening on TCP port {settings.Port}[/]", settings);

            var successResult = new PortListenerResult
            {
                Success = true,
                Port = settings.Port,
                ReceivedData = receivedData,
                ResultsFile = settings.ResultsFile
            };
            WriteResult(successResult, settings);

            return 0;
        }
        catch (OperationCanceledException)
        {
            var result = new PortListenerResult
            {
                Success = true,
                Port = settings.Port,
                ErrorMessage = "Operation cancelled"
            };

            WriteConsoleOutput($"[yellow]Stopped listening on TCP port {settings.Port}[/]", settings);
            WriteResult(result, settings);
            return 0;
        }
        catch (Exception ex)
        {
            var result = new PortListenerResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Port = settings.Port
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}