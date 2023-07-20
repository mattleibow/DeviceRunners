using System.Reflection;

namespace DeviceRunners.XHarness;

public static class XHarnessTestRunnerConfigurationBuilderExtensions
{
	public static TBuilder AddTestAssembly<TBuilder>(this TBuilder builder, Assembly assembly)
		where TBuilder : IXHarnessTestRunnerConfigurationBuilder
	{
		builder.AddTestAssembly(assembly);
		return builder;
	}

	public static TBuilder AddTestAssemblies<TBuilder>(this TBuilder builder, IEnumerable<Assembly> assemblies)
		where TBuilder : IXHarnessTestRunnerConfigurationBuilder
	{
		foreach (var assembly in assemblies)
			builder.AddTestAssembly(assembly);
		return builder;
	}

	public static TBuilder AddTestAssemblies<TBuilder>(this TBuilder builder, params Assembly[] assemblies)
		where TBuilder : IXHarnessTestRunnerConfigurationBuilder
	{
		foreach (var assembly in assemblies)
			builder.AddTestAssembly(assembly);
		return builder;
	}

	public static TBuilder SkipCategory<TBuilder>(this TBuilder builder, string category, string skipValue)
		where TBuilder : IXHarnessTestRunnerConfigurationBuilder
	{
		builder.SkipCategory(category, skipValue);
		return builder;
	}

	public static TBuilder UseEnvironmentVariables<TBuilder>(this TBuilder builder)
		where TBuilder : IXHarnessTestRunnerConfigurationBuilder
	{
		var path = AdditionalApplicationOptions.Current.OutputDirectory;
		if (!string.IsNullOrEmpty(path))
			builder.SetOutputDirectory(path);
		return builder;
	}
}
