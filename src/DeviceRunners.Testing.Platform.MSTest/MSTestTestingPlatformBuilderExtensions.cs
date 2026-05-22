using System.Reflection;

using Microsoft.Testing.Platform.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Registers MSTest as the test framework for the MTP device runner.
/// </summary>
public static class MSTestTestingPlatformBuilderExtensions
{
	/// <summary>
	/// Registers MSTest as the test framework.
	/// Uses builder.AddMSTest(() => assemblies) from the MSTest.TestAdapter package.
	/// </summary>
	public static TBuilder AddMSTestFramework<TBuilder>(this TBuilder builder, Assembly? testAssembly = null)
		where TBuilder : ITestingPlatformRunnerConfigurationBuilder
	{
		testAssembly ??= Assembly.GetCallingAssembly();

		builder.UseTestFramework(async (args, configureExtensions) =>
		{
			var mtpBuilder = await TestApplication.CreateBuilderAsync(args);
			mtpBuilder.AddMSTest(() => [testAssembly]);
			configureExtensions(mtpBuilder);
			using var app = await mtpBuilder.BuildAsync();
			return await app.RunAsync();
		});

		return builder;
	}
}
