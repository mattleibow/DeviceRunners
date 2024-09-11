using System.Diagnostics.CodeAnalysis;

using Microsoft.Playwright;

namespace DeviceRunners.UIAutomation.Playwright;

internal static class PlaywrightBrowserLaunchOptionsExtensions
{
	public static void SetBrowserType(this PlaywrightBrowserLaunchOptions options, string browserType) =>
		options[PlaywrightBrowserLaunchOptionKeys.BrowserType] = browserType;

	public static string GetBrowserType(this IPlaywrightBrowserLaunchOptions options) =>
		(string)options.GetOption(PlaywrightBrowserLaunchOptionKeys.BrowserType);

	public static void SetBrowserTypeLaunchOptions(this PlaywrightBrowserLaunchOptions options, BrowserTypeLaunchOptions launchOptions) =>
		options[PlaywrightBrowserLaunchOptionKeys.BrowserTypeLaunchOptions] = launchOptions;

	public static BrowserTypeLaunchOptions? GetBrowserTypeLaunchOptions(this IPlaywrightBrowserLaunchOptions options) =>
		(BrowserTypeLaunchOptions?)options.GetOption(PlaywrightBrowserLaunchOptionKeys.BrowserTypeLaunchOptions);

	public static BrowserTypeLaunchOptions GetOrAddBrowserTypeLaunchOptions(this PlaywrightBrowserLaunchOptions options)
	{
		if (!options.TryGetValue(PlaywrightBrowserLaunchOptionKeys.BrowserTypeLaunchOptions, out var launchOptions))
			options[PlaywrightBrowserLaunchOptionKeys.BrowserTypeLaunchOptions] = launchOptions = new BrowserTypeLaunchOptions();

		return (BrowserTypeLaunchOptions)launchOptions;
	}

	private static bool TryGetOption(this IPlaywrightBrowserLaunchOptions options, string name, [NotNullWhen(true)] out object? value)
	{
		var obj = options.GetOption(name);
		if (obj is null)
		{
			value = null;
			return false;
		}

		value = obj;
		return true;
	}

	private static bool TryGetStringOption(this IPlaywrightBrowserLaunchOptions options, string name, [NotNullWhen(true)] out string? value)
	{
		if (!options.TryGetOption(name, out var objVal) ||
			objVal.ToString() is not string strVal ||
			string.IsNullOrWhiteSpace(strVal))
		{
			value = null;
			return false;
		}

		value = strVal;
		return true;
	}

	public static bool TryGetInitialUrl(this IPlaywrightBrowserLaunchOptions options, [NotNullWhen(true)] out string? initialUrl)
	{
		if (!options.TryGetStringOption(PlaywrightBrowserLaunchOptionKeys.InitialUrl, out var url))
		{
			initialUrl = null;
			return false;
		}

		initialUrl = url;
		return true;
	}
}
