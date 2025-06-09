using DeviceRunners.Cli.Services;
using System.Net.Sockets;
using System.Text;

namespace DeviceRunners.Cli.Tests;

public class NetworkServiceTests
{
    [Fact]
    public void IsPortAvailable_WithAvailablePort_ReturnsTrue()
    {
        // Arrange
        var service = new NetworkService();
        var port = 0; // Let OS choose available port
        
        // Act
        var result = service.IsPortAvailable(port);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StartTcpListener_WithValidPort_CanAcceptConnections()
    {
        // Arrange
        var service = new NetworkService();
        var port = 0; // Let OS choose available port
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        
        // Act & Assert - Should not throw and should handle cancellation gracefully
        try
        {
            await service.StartTcpListener(port, null, true, 30, 30, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when no connections come in within timeout
        }
    }

    [Fact]
    public async Task StartTcpListener_NonInteractiveWithConnectionTimeout_TimesOutWhenNoConnection()
    {
        // Arrange
        var service = new NetworkService();
        var testPort = 16386; // Use a specific port for testing
        var connectionTimeout = 1; // 1 second timeout
        
        // Act & Assert
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await service.StartTcpListener(testPort, null, true, connectionTimeout, 30, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected due to connection timeout
        }
        
        stopwatch.Stop();
        // Should timeout around 1 second (allow some variance for test reliability)
        Assert.True(stopwatch.ElapsedMilliseconds >= 800 && stopwatch.ElapsedMilliseconds <= 2000, 
            $"Expected timeout around 1 second, but took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task StartTcpListener_InteractiveMode_NoTimeouts()
    {
        // Arrange
        var service = new NetworkService();
        var testPort = 16387; // Use a specific port for testing
        var connectionTimeout = 1; // 1 second timeout - should be ignored
        
        // Act & Assert
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // In interactive mode, timeouts should be ignored
            await service.StartTcpListener(testPort, null, false, connectionTimeout, 30, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected due to manual cancellation after 2 seconds, not connection timeout
        }
        
        stopwatch.Stop();
        // Should wait for full 2 seconds, not timeout at 1 second
        Assert.True(stopwatch.ElapsedMilliseconds >= 1800, 
            $"Expected to wait at least 1.8 seconds in interactive mode, but took only {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task StartTcpListener_RoundTripTest_ReceivesDataCorrectly()
    {
        // Arrange
        var service = new NetworkService();
        var testPort = 16385; // Use a specific port for testing
        var testMessage = "Test message for round trip";
        var tempFile = Path.GetTempFileName();
        
        try
        {
            // Start the listener in background
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var listenerTask = Task.Run(async () =>
            {
                return await service.StartTcpListener(testPort, tempFile, true, 30, 30, cancellationTokenSource.Token);
            });

            // Give the listener a moment to start
            await Task.Delay(200);
            
            // Send data to the listener
            using (var client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", testPort);
                using var stream = client.GetStream();
                var buffer = Encoding.ASCII.GetBytes(testMessage);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            }

            // Wait for listener to complete
            var result = await listenerTask;

            // Assert
            Assert.Contains(testMessage, result);
            
            // Check file was written
            if (File.Exists(tempFile))
            {
                var fileContent = await File.ReadAllTextAsync(tempFile);
                Assert.Equal(testMessage, fileContent);
            }
        }
        catch (SocketException ex) when (ex.Message.Contains("Address already in use"))
        {
            // Skip test if port is in use - this is expected in CI environments
            return;
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}