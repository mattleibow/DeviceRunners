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
		using var console = new TestConsole();
		var app = new CommandApp();

		app.Configure(config =>
		{
			config.ConfigureConsole(console);
			config.PropagateExceptions();

			foreach (var configure in _configurations)
				configure(config);
		});

		try
		{
			var exitCode = app.Run(args);
			return new CommandAppResult(exitCode, console.Output);
		}
		catch (Exception ex)
		{
			return new CommandAppResult(-1, console.Output + ex.Message);
		}
	}
}

public sealed class CommandAppResult(int exitCode, string output)
{
	public int ExitCode => exitCode;
	public string Output => output;
}
