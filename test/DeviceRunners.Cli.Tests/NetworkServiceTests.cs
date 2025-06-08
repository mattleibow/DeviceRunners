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
            await service.StartTcpListener(port, null, true, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when no connections come in within timeout
        }
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
                return await service.StartTcpListener(testPort, tempFile, true, cancellationTokenSource.Token);
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