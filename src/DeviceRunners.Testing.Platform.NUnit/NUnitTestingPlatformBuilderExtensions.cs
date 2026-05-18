using System.Reflection;

using Microsoft.Testing.Platform.Builder;
using NUnit.VisualStudio.TestAdapter.TestingPlatformAdapter;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Registers NUnit as the test framework for the MTP device runner.
/// </summary>
public static class NUnitTestingPlatformBuilderExtensions
{
	/// <summary>
	/// Registers NUnit as the test framework.
	/// Uses builder.AddNUnit(() => assemblies) from NUnit3TestAdapter.
	/// </summary>
	public static TBuilder AddNUnitFramework<TBuilder>(this TBuilder builder, Assembly? testAssembly = null)
		where TBuilder : ITestingPlatformRunnerConfigurationBuilder
	{
		testAssembly ??= Assembly.GetCallingAssembly();

		builder.UseTestFramework(async (args, configureExtensions) =>
		{
			var mtpBuilder = await TestApplication.CreateBuilderAsync(args);
			mtpBuilder.AddNUnit(() => [testAssembly]);
			configureExtensions(mtpBuilder);
			using var app = await mtpBuilder.BuildAsync();
			return await app.RunAsync();
		});

		return builder;
	}
}
