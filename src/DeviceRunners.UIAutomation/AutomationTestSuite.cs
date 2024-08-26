﻿using System.Collections.Concurrent;

namespace DeviceRunners.UIAutomation;

public class AutomationTestSuite : IDisposable
{
	private readonly IReadOnlyList<IAutomationFramework> _automationFrameworks;
	private readonly IReadOnlyDictionary<string, AvailableApp> _availableApps;
	private readonly ConcurrentDictionary<string, InstantiatedApp> _instantiatedApps = new();
	private bool _disposed;

	private record class AvailableApp(string Key, IAutomationFramework Framework, IAutomatedAppOptions Options);

	private record class InstantiatedApp(string Key, IAutomationFramework Framework, IAutomatedApp App);

	public AutomationTestSuite(IReadOnlyList<IAutomationFramework> automationFrameworks)
	{
		_automationFrameworks = automationFrameworks;
		_availableApps = CollectAvailableApps();
	}

	public IAutomatedApp GetApp(string appKey) =>
		GetApp(appKey, false, false);

	public IAutomatedApp StartApp(string appKey) =>
		GetApp(appKey, true, false);

	public void StopApp(string appKey)
	{
		if (_instantiatedApps.TryGetValue(appKey, out var instantiatedApp))
			instantiatedApp.Framework.StopApp(instantiatedApp.App);
	}

	public IAutomatedApp RestartApp(string appKey) =>
		GetApp(appKey, true, true);

	private IAutomatedApp GetApp(string appKey, bool start, bool restart)
	{
		ObjectDisposedException.ThrowIf(_disposed, typeof(AutomationTestSuite));

		var instantiatedApp = _instantiatedApps.AddOrUpdate(
			appKey,
			_ =>
			{
				if (!_availableApps.TryGetValue(appKey, out var appOptions))
					throw new KeyNotFoundException($"App with key '{appKey}' was not found.");
				
				var framework = appOptions.Framework;
				var app = framework.CreateApp(appOptions.Options);

				if (start)
					framework.StartApp(app);

				return new InstantiatedApp(appKey, framework, app);
			},
			(_, instantiated) =>
			{
				var framework = instantiated.Framework;
				var app = instantiated.App;

				if (restart)
					framework.RestartApp(app);
				
				return instantiated;
			});

		return instantiatedApp.App;
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		// close all apps
		foreach (var app in _instantiatedApps.Values)
		{
			app.App.Dispose();
		}
		_instantiatedApps.Clear();

		// shutdown frameworks
		foreach (var framework in _automationFrameworks)
		{
			framework.Dispose();
		}
	}

	private IReadOnlyDictionary<string, AvailableApp> CollectAvailableApps()
	{
		var availableApps = new Dictionary<string, AvailableApp>();

		foreach (var framework in _automationFrameworks)
		{
			foreach (var app in framework.AvailableApps)
			{
				availableApps[app.Key] = new AvailableApp(app.Key, framework, app);
			}
		}

		return availableApps;
	}
}
