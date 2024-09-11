using System.Diagnostics.CodeAnalysis;

using OpenQA.Selenium;

namespace DeviceRunners.UIAutomation.Selenium;

internal static class DriverOptionsExtensions
{
	internal const string DriverOptionPrefix = "devicerunners:";
	internal const string InitialUrlDriverOption = "initialUrl";

	private static bool TryGetCapability(this DriverOptions options, string name, [NotNullWhen(true)] out object? value)
	{
		var obj = options.ToCapabilities().GetCapability(DriverOptionPrefix + name);
		if (obj is null)
		{
			value = null;
			return false;
		}

		value = obj;
		return true;
	}

	private static bool TryGetStringCapability(this DriverOptions options, string name, [NotNullWhen(true)] out string? value)
	{
		if (!options.TryGetCapability(name, out var objVal) ||
			objVal.ToString() is not string strVal ||
			string.IsNullOrWhiteSpace(strVal))
		{
			value = null;
			return false;
		}

		value = strVal;
		return true;
	}

	public static bool TryGetInitialUrl(this DriverOptions options, [NotNullWhen(true)] out string? initialUrl)
	{
		if (!options.TryGetStringCapability(InitialUrlDriverOption, out var url))
		{
			initialUrl = null;
			return false;
		}

		initialUrl = url;
		return true;
	}
}
