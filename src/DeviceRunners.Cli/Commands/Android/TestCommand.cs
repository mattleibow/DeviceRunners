using System.ComponentModel;
using DeviceRunners.Cli.Models;
using DeviceRunners.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public class AndroidTestCommand(IAnsiConsole console) : BaseTestCommand<AndroidTestCommand.Settings>(console)
{
    public class Settings : BaseTestCommandSettings
    {
        [Description("Package name to launch")]
        [CommandOption("--package")]
        public string? Package { get; set; }
        
        [Description("Main activity name (defaults to <package>.MainActivity)")]
        [CommandOption("--activity")]
        public string? Activity { get; set; }
        
        [Description("Device or emulator serial number")]
        [CommandOption("--device")]
        public string? Device { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]PREPARATION[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            var androidService = new AndroidService();
            var networkService = new NetworkService();
            
            // Clear logcat before starting
            WriteConsoleOutput($"  - Clearing logcat...", settings);
            androidService.ClearLogcat(settings.Device);
            WriteConsoleOutput($"    Logcat cleared.", settings);
            
            // Display device info if specified
            if (!string.IsNullOrEmpty(settings.Device))
            {
                WriteConsoleOutput($"  - Using device: [green]{Markup.Escape(settings.Device)}[/]", settings);
            }

            string packageName;
            if (settings.App is not null)
            {
                // Install the APK
                WriteConsoleOutput($"  - Installing APK: [green]{Markup.Escape(settings.App)}[/]", settings);
                androidService.InstallApk(settings.App, settings.Device);
                WriteConsoleOutput($"    APK installed successfully.", settings);

                // Determine package name from APK
                WriteConsoleOutput($"  - Determining package name from APK...", settings);
                packageName = androidService.GetPackageName(settings.App);
                WriteConsoleOutput($"    APK file: '[green]{Markup.Escape(settings.App)}[/]'", settings);
                WriteConsoleOutput($"    Package name found: '[green]{Markup.Escape(packageName)}[/]'", settings);

            }
            else if (!string.IsNullOrEmpty(settings.Package))
            {
                packageName = settings.Package;
                WriteConsoleOutput($"  - Using specified package: [green]{Markup.Escape(packageName)}[/]", settings);
            }
            else
            {
                var result = new TestStartResult
                {
                    Success = false,
                    ErrorMessage = "Either --app or --package must be specified"
                };

                WriteConsoleOutput($"[red]Error: Either --app or --package must be specified[/]", settings);
                WriteResult(result, settings);

                return 1;
            }

            // Start the app
            WriteConsoleOutput($"  - Starting the application...", settings);
            androidService.LaunchApp(packageName, settings.Activity, settings.Device);
            WriteConsoleOutput($"    Application started.", settings);

            // Handle TCP test results
            var (testFailures, testResults) = await StartTestListener(settings);

            WriteConsoleOutput($"", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);
            WriteConsoleOutput($"[blue]CLEANUP[/]", settings);
            WriteConsoleOutput($"[blue]============================================================[/]", settings);

            // Save logcat after test run
            var logcatFile = GetLogcatFilePath(settings);
            WriteConsoleOutput($"  - Saving logcat to: [green]{Markup.Escape(logcatFile)}[/]", settings);
            androidService.SaveLogcat(logcatFile, settings.Device);
            WriteConsoleOutput($"    Logcat saved.", settings);

            WriteConsoleOutput($"  - Cleanup complete.", settings);

            var testResult = new TestStartResult
            {
                Success = testFailures == 0,
                AppIdentity = packageName,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory,
                TestFailures = testFailures,
                TestResults = testResults,
                LogcatFile = logcatFile
            };
            WriteResult(testResult, settings);

            return testFailures > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            var androidService = new AndroidService();
            var logcatFile = GetLogcatFilePath(settings);
            
            try
            {
                WriteConsoleOutput($"  - Saving logcat due to error: [green]{Markup.Escape(logcatFile)}[/]", settings);
                androidService.SaveLogcat(logcatFile, settings.Device);
                WriteConsoleOutput($"    Logcat saved.", settings);
            }
            catch (Exception logcatEx)
            {
                WriteConsoleOutput($"[yellow]Warning: Failed to save logcat: {Markup.Escape(logcatEx.Message)}[/]", settings);
            }

            var result = new TestStartResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                AppPath = settings.App,
                ResultsDirectory = settings.ResultsDirectory,
                LogcatFile = logcatFile
            };

            WriteConsoleOutput($"[red]Error: {Markup.Escape(ex.Message)}[/]", settings);
            WriteResult(result, settings);
            return 1;
        }
    }

    private string GetLogcatFilePath(Settings settings)
    {
        var resultsDir = settings.ResultsDirectory ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(resultsDir);
        return Path.Combine(resultsDir, "logcat.txt");
    }
}
