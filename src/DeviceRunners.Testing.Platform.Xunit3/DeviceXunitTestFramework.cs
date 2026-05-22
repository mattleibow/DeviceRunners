using System.Reflection;

using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Requests;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// ITestFramework for xunit v3 that works on platforms where Assembly.GetEntryAssembly()
/// returns null (Android, iOS). Uses an explicit assembly reference.
/// This is used when TestPlatformTestFramework.RunAsync() cannot be used.
/// </summary>
sealed class DeviceXunitTestFramework : ITestFramework, IDataProducer
{
	readonly IServiceProvider _serviceProvider;
	readonly Assembly _testAssembly;

	public DeviceXunitTestFramework(IServiceProvider serviceProvider, Assembly testAssembly)
	{
		_serviceProvider = serviceProvider;
		_testAssembly = testAssembly;
	}

	public string Uid => "DeviceRunners.Xunit3";
	public string Version => "1.0.0";
	public string DisplayName => "Device xUnit v3";
	public string Description => "xUnit v3 on device via DeviceRunners";

	public Task<bool> IsEnabledAsync() => Task.FromResult(true);

	public Type[] DataTypesProduced => [typeof(TestNodeUpdateMessage)];

	public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context)
		=> Task.FromResult(new CreateTestSessionResult { IsSuccess = true });

	public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
		=> Task.FromResult(new CloseTestSessionResult { IsSuccess = true });

	public async Task ExecuteRequestAsync(ExecuteRequestContext context)
	{
		try
		{
			// TODO: Full implementation using ProjectAssemblyRunner with:
			// - XunitProject + XunitProjectAssembly configured for _testAssembly
			// - InMemoryXunit3TestAssembly for platforms with empty Assembly.Location
			// - Custom message sink translating IMessageSinkMessage → TestNodeUpdateMessage
			// - Handles both DiscoverTestExecutionRequest and RunTestExecutionRequest
			//
			// For now, report that no tests were found (allows compilation and integration testing).
			await Task.CompletedTask;
		}
		finally
		{
			context.Complete();
		}
	}
}
