using DeviceRunners.Cli.Commands;
using DeviceRunners.Cli.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Net.Sockets;
using System.Text;

namespace DeviceRunners.Cli.Tests;

public class BaseTestCommandTests
{
    private class TestableTestCommand : BaseTestCommand<TestableTestCommand.Settings>
    {
        public class Settings : BaseTestCommandSettings
        {
            // App is inherited from BaseTestCommandSettings
        }

        public TestableTestCommand(IAnsiConsole console) : base(console) { }

        protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            // This is a test implementation that simulates the TCP listener timing behavior
            return await Task.FromResult(0);
        }

        // Expose the protected methods for testing
        public async Task<Task<(int testFailures, string? testResults)>> TestStartTestListenerBackground(Settings settings) =>
            await StartTestListenerBackground(settings);
    }

    [Fact]
    public async Task StartTestListenerBackground_ReturnsQuickly_DemonstratesTimingFix()
    {
        // Arrange
        var console = new TestConsole();
        var command = new TestableTestCommand(console);
        var settings = new TestableTestCommand.Settings
        {
            App = "test.msix",
            Port = 16391, // Use a unique port for this test
            ConnectionTimeout = 5, // Short timeout
            DataTimeout = 5,
            ResultsDirectory = "/tmp/test-results"
        };

        // Act - This should return quickly (demonstrating the fix)
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var listenerTask = await command.TestStartTestListenerBackground(settings);
        stopwatch.Stop();

        // Assert - The method should return quickly with listener ready
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"StartTestListenerBackground took {stopwatch.ElapsedMilliseconds}ms, should return quickly");

        Assert.False(listenerTask.IsCompleted, "Listener task should still be running in background");

        // The timing fix means the listener is ready immediately, 
        // so an app can connect without waiting
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", settings.Port);
            // Connection succeeded - this demonstrates the timing fix
            
            // Send minimal data to complete the test
            using var stream = client.GetStream();
            var buffer = Encoding.ASCII.GetBytes("Failed: 0");
            await stream.WriteAsync(buffer);
            await stream.FlushAsync();
            
            // Wait for completion
            await listenerTask;
        }
        catch (Exception ex)
        {
            Assert.True(false, $"Should be able to connect immediately after StartTestListenerBackground, but got: {ex.Message}");
        }
    }
}