using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace DeviceRunners.Cli.Tests;

/// <summary>
/// Replacement for the removed Spectre.Console.Testing.CommandAppTester.
/// Uses CommandApp with a TestConsole to capture output.
/// </summary>
public sealed class CommandAppTester
{
	readonly List<Action<IConfigurator>> _configurations = [];

	public void Configure(Action<IConfigurator> configure)
	{
		_configurations.Add(configure);
	}

	public CommandAppResult Run(params string[] args)
	{
		var console = new TestConsole();
		console.Profile.Width = 10000;
		var app = new CommandApp();

		app.Configure(config =>
		{
			config.ConfigureConsole(console);

			foreach (var configure in _configurations)
				configure(config);
		});

		var exitCode = app.Run(args);
		return new CommandAppResult(exitCode, console.Output);
	}
}

public sealed class CommandAppResult(int exitCode, string output)
{
	public int ExitCode => exitCode;
	public string Output => output;
}
