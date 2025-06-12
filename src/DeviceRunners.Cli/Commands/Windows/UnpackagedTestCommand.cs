using System.ComponentModel;
using System.Diagnostics;
using DeviceRunners.Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class WindowsUnpackagedTestCommand(IAnsiConsole console) : BaseTestCommand<WindowsUnpackagedTestCommand.Settings>(console)
{
	public class Settings : BaseTestCommandSettings
    {
        // No additional settings needed for unpackaged apps
        // The base class already provides --app, --port, --results-directory, etc.
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]PREPARATION[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            WriteConsoleOutput($"  - Validating unpackaged application...", settings);
            
            // Validate that the app file exists and is an executable
            if (!File.Exists(settings.App))
            {
                throw new FileNotFoundException($"Application file not found: {settings.App}");
            }

            var extension = Path.GetExtension(settings.App).ToLowerInvariant();
            if (extension != ".exe")
            {
                throw new InvalidOperationException($"Unpackaged apps must be .exe files. Found: {extension}");
            }

            WriteConsoleOutput($"    Application file: '[green]{Markup.Escape(settings.App)}[/]'", settings);
            WriteConsoleOutput($"    Application validated.", settings);

            WriteConsoleOutput($"", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]EXECUTION[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            // Start the unpackaged application directly
            WriteConsoleOutput($"  - Starting the unpackaged application...", settings);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = settings.App,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(settings.App)
            };

            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the application process.");
            }

            WriteConsoleOutput($"    Application started with PID: {process.Id}", settings);

            // Handle TCP test results
            var (testFailures, testResults) = await StartTestListener(settings);

            WriteConsoleOutput($"", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            // For unpackaged apps, we just try to terminate the process if it's still running
            WriteConsoleOutput($"  - Checking application process...", settings);
            try
            {
                if (!process.HasExited)
                {
                    WriteConsoleOutput($"    Application is still running, terminating...", settings);
                    process.Kill();
                    process.WaitForExit(5000); // Wait up to 5 seconds for graceful exit
                    WriteConsoleOutput($"    Application terminated.", settings);
                }
                else
                {
                    WriteConsoleOutput($"    Application has already exited.", settings);
                }
            }
            catch (Exception ex)
            {
                WriteConsoleOutput($"    [yellow]Warning: Failed to check/terminate application process: {Markup.Escape(ex.Message)}[/]", settings);
            }

            WriteConsoleOutput($"  - Cleanup complete.", settings);

            var result = new TestStartResult
            {
                Success = testFailures == 0,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory,
                TestFailures = testFailures,
                TestResults = testResults
            };
            WriteResult(result, settings);

            return testFailures > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            var result = new TestStartResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }
}