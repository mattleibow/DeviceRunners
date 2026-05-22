using System.Reflection;

using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;

using Xunit.MicrosoftTestingPlatform;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Registers xunit v3 as the test framework for the MTP device runner.
/// </summary>
public static class Xunit3TestingPlatformBuilderExtensions
{
	/// <summary>
	/// Registers xunit v3 as the test framework.
	/// Auto-detects platform:
	/// - Desktop (GetEntryAssembly non-null): uses TestPlatformTestFramework.RunAsync (xunit owns lifecycle)
	/// - Mobile (GetEntryAssembly null): uses DeviceXunitTestFramework with explicit assembly
	/// </summary>
	public static TBuilder AddXunit3<TBuilder>(this TBuilder builder, Assembly? testAssembly = null)
		where TBuilder : ITestingPlatformRunnerConfigurationBuilder
	{
		testAssembly ??= Assembly.GetCallingAssembly();

		builder.UseTestFramework(async (args, configureExtensions) =>
		{
			if (Assembly.GetEntryAssembly() is not null)
			{
				// Desktop path: xunit owns the MTP lifecycle
				return await TestPlatformTestFramework.RunAsync(args, (mtpBuilder, _) =>
				{
					configureExtensions(mtpBuilder);
				});
			}

			// Mobile path (Android/iOS): GetEntryAssembly() is null
			var mtpBuilder = await TestApplication.CreateBuilderAsync(args);
			mtpBuilder.RegisterTestFramework(
				_ => new DeviceXunitTestFrameworkCapabilities(),
				(_, sp) => new DeviceXunitTestFramework(sp, testAssembly));
			configureExtensions(mtpBuilder);
			using var app = await mtpBuilder.BuildAsync();
			return await app.RunAsync();
		});

		return builder;
	}
}
