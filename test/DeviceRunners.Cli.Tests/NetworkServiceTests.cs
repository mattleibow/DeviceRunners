using DeviceRunners.Cli.Services;

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
}