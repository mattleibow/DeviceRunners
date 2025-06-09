using DeviceRunners.Cli.Services;
using System.Net.Sockets;
using System.Text;
using System.Net;

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
        var eventLog = service.CreateEventLog();
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
            await NetworkServiceTestExtensions.ConnectAndSendAsync(testPort, testMessage);

            // Wait for listener to complete
            var result = await listenerTask;

            // Assert
            Assert.Contains(testMessage, result);
            
            // Check events were fired
            Assert.True(eventLog.HasConnectionEstablished);
            Assert.True(eventLog.HasConnectionClosed);
            Assert.Contains(testMessage, eventLog.DataReceived);
            
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

    [Fact]
    public async Task StartTcpListener_EventsTest_EmitsCorrectEvents()
    {
        // Arrange
        var service = new NetworkService();
        var eventLog = service.CreateEventLog();
        var testPort = 16386; // Use a different port for this test
        var testMessage = "Test message for events";
        
        try
        {
            // Start the listener in background
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var listenerTask = Task.Run(async () =>
            {
                return await service.StartTcpListener(testPort, null, true, cancellationTokenSource.Token);
            });

            // Give the listener a moment to start
            await Task.Delay(200);
            
            // Send data to the listener
            await NetworkServiceTestExtensions.ConnectAndSendAsync(testPort, testMessage);

            // Wait for listener to complete
            var result = await listenerTask;

            // Assert events were fired
            Assert.True(eventLog.HasConnectionEstablished);
            Assert.True(eventLog.HasConnectionClosed);
            Assert.Contains(testMessage, eventLog.DataReceived);
        }
        catch (SocketException ex) when (ex.Message.Contains("Address already in use"))
        {
            // Skip test if port is in use - this is expected in CI environments
            return;
        }
    }

    [Fact]
    public async Task StartTcpListener_ConnectWithoutData_EmitsConnectionEvents()
    {
        // Arrange
        var service = new NetworkService();
        var eventLog = service.CreateEventLog();
        var testPort = 16387;
        
        try
        {
            // Start the listener in background
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var listenerTask = Task.Run(async () =>
            {
                return await service.StartTcpListener(testPort, null, true, cancellationTokenSource.Token);
            });

            // Give the listener a moment to start
            await Task.Delay(200);
            
            // Connect but don't send data
            await NetworkServiceTestExtensions.ConnectDisconnectAsync(testPort);

            // Wait for listener to timeout or complete
            try
            {
                await listenerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when no data is sent
            }

            // Assert connection events were fired but no data events
            Assert.True(eventLog.HasConnectionEstablished);
            Assert.True(eventLog.HasConnectionClosed);
            Assert.Empty(eventLog.DataReceived);
        }
        catch (SocketException ex) when (ex.Message.Contains("Address already in use"))
        {
            // Skip test if port is in use - this is expected in CI environments
            return;
        }
    }

    [Fact]
    public async Task StartTcpListener_SingleMessage_ReceivesCorrectly()
    {
        // Arrange
        var service = new NetworkService();
        var eventLog = service.CreateEventLog();
        var testPort = 16388;
        var testMessage = "Single message";
        
        try
        {
            // Use the helper method to start listener and send message
            var result = await service.StartBackgroundListener(testPort, async () =>
            {
                await NetworkServiceTestExtensions.ConnectAndSendAsync(testPort, testMessage);
            });

            // Assert events and data
            Assert.True(eventLog.HasConnectionEstablished);
            Assert.True(eventLog.HasConnectionClosed);
            Assert.Single(eventLog.DataReceived);
            Assert.Equal(testMessage, eventLog.DataReceived[0]);
            Assert.Contains(testMessage, result);
        }
        catch (SocketException ex) when (ex.Message.Contains("Address already in use"))
        {
            // Skip test if port is in use - this is expected in CI environments
            return;
        }
    }

    [Fact]
    public async Task StartTcpListener_MultipleMessages_ReceivesAllCorrectly()
    {
        // Arrange
        var service = new NetworkService();
        var eventLog = service.CreateEventLog();
        var testPort = 16389;
        var messages = new[] { "Message 1", "Message 2", "Message 3" };
        
        try
        {
            // Start the listener in background
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var listenerTask = Task.Run(async () =>
            {
                return await service.StartTcpListener(testPort, null, true, cancellationTokenSource.Token);
            });

            // Give the listener a moment to start
            await Task.Delay(200);
            
            // Send multiple messages
            await NetworkServiceTestExtensions.ConnectAndSendAsync(testPort, messages);

            // Wait for listener to complete
            var result = await listenerTask;

            // Assert events and data
            Assert.True(eventLog.HasConnectionEstablished);
            Assert.True(eventLog.HasConnectionClosed);
            
            // TCP may combine messages into fewer chunks, so verify total data received
            var allReceivedData = string.Join("", eventLog.DataReceived);
            var expectedResult = string.Join("", messages);
            Assert.Equal(expectedResult, allReceivedData);
            Assert.Contains(expectedResult, result);
        }
        catch (SocketException ex) when (ex.Message.Contains("Address already in use"))
        {
            // Skip test if port is in use - this is expected in CI environments
            return;
        }
    }

    [Fact]
    public async Task StartTcpListener_NoConnection_TimesOutGracefully()
    {
        // Arrange
        var service = new NetworkService();
        var eventLog = service.CreateEventLog();
        var testPort = 16390;
        
        try
        {
            // Start the listener with short timeout
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            
            // Act & Assert - should handle timeout gracefully
            try
            {
                await service.StartTcpListener(testPort, null, true, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when no connections come in within timeout
            }

            // No events should have been fired
            Assert.Empty(eventLog.Events);
            Assert.Empty(eventLog.DataReceived);
        }
        catch (SocketException ex) when (ex.Message.Contains("Address already in use"))
        {
            // Skip test if port is in use - this is expected in CI environments
            return;
        }
    }
}

public static class NetworkServiceTestExtensions
{
    public static NetworkEventLog CreateEventLog(this NetworkService service)
    {
        var eventLog = new NetworkEventLog();
        
        service.ConnectionEstablished += eventLog.LogConnectionEstablished;
        service.ConnectionClosed += eventLog.LogConnectionClosed;
        service.DataReceived += eventLog.LogDataReceived;
        
        return eventLog;
    }

    public static async Task<string> StartBackgroundListener(this NetworkService service, int port, Func<Task> action, string? outputPath = null, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        
        try
        {
            // Start the listener in background
            var cancellationTokenSource = new CancellationTokenSource(timeout.Value);
            var listenerTask = Task.Run(async () =>
            {
                return await service.StartTcpListener(port, outputPath, true, cancellationTokenSource.Token);
            });

            // Give the listener a moment to start
            await Task.Delay(200);
            
            // Execute the action (send messages, connect, etc.)
            await action();

            // Wait for listener to complete
            return await listenerTask;
        }
        catch (SocketException ex) when (ex.Message.Contains("Address already in use"))
        {
            // Re-throw - caller will handle this as expected in CI environments
            throw;
        }
    }

    public static async Task ConnectAndSendAsync(int port, params string[] messages)
    {
        if (messages.Length == 0) return;
        
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);
        using var stream = client.GetStream();
        
        foreach (var message in messages)
        {
            var buffer = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
        }
    }
    
    public static async Task ConnectDisconnectAsync(int port)
    {
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);
        // Just connect and disconnect immediately
    }

    public class NetworkEventLog
    {
        public List<string> Events { get; } = new();
        public List<string> DataReceived { get; } = new();
        
        public void LogConnectionEstablished(object? sender, ConnectionEventArgs e)
        {
            Events.Add($"ConnectionEstablished:{e.RemoteEndPoint}");
        }
        
        public void LogConnectionClosed(object? sender, ConnectionEventArgs e)
        {
            Events.Add($"ConnectionClosed:{e.RemoteEndPoint}");
        }
        
        public void LogDataReceived(object? sender, DataReceivedEventArgs e)
        {
            Events.Add($"DataReceived:{e.RemoteEndPoint}:{e.Data}");
            DataReceived.Add(e.Data);
        }
        
        public bool HasConnectionEstablished => Events.Any(e => e.StartsWith("ConnectionEstablished:"));
        public bool HasConnectionClosed => Events.Any(e => e.StartsWith("ConnectionClosed:"));
        public bool HasDataReceived => Events.Any(e => e.StartsWith("DataReceived:"));
    }
}