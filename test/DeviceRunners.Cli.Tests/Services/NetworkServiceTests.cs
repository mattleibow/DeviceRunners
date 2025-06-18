using DeviceRunners.Cli.Services;
using System.Net.Sockets;
using System.Text;

namespace DeviceRunners.Cli.Tests;

public class NetworkServiceTests : IDisposable
{
    private readonly NetworkService _service;
    private readonly NetworkServiceEventLog _eventLog;
    private string? _tempPath;

    public NetworkServiceTests()
    {
        _service = new NetworkService();
        _eventLog = CreateEventLog();
    }

    public void Dispose()
    {
        if (!string.IsNullOrWhiteSpace(_tempPath) && File.Exists(_tempPath))
        {
            File.Delete(_tempPath);
        }
    }

    [Fact]
    public void IsPortAvailable_WithAvailablePort_ReturnsTrue()
    {
        // Arrange
        var port = 0; // Let OS choose available port

        // Act
        var result = _service.IsPortAvailable(port);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StartTcpListener_WithValidPort_CanAcceptConnections()
    {
        // Arrange
        var port = 0; // Let OS choose available port
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        // Act & Assert - Should not throw and should handle cancellation gracefully
        try
        {
            await _service.StartTcpListener(port, null, true, 30, 30, cancellationTokenSource.Token);
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
        var testPort = 16386; // Use a specific port for testing
        var connectionTimeout = 1; // 1 second timeout
        
        // Act & Assert
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await _service.StartTcpListener(testPort, null, true, connectionTimeout, 30, cancellationTokenSource.Token);
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
        var testPort = 16387; // Use a specific port for testing
        var connectionTimeout = 1; // 1 second timeout - should be ignored
        
        // Act & Assert
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // In interactive mode, timeouts should be ignored
            await _service.StartTcpListener(testPort, null, false, connectionTimeout, 30, cancellationTokenSource.Token);
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
        var testPort = 16385; // Use a specific port for testing
        var testMessage = "Test message for round trip";
        _tempPath = Path.GetTempFileName();

        // Act - Start the listener and send a single message
        var result = await StartBackgroundListener(testPort, null, () =>
            ConnectAndSendAsync(testPort, testMessage));

        // Assert
        Assert.Contains(testMessage, result);

        // Check events were fired
        Assert.True(_eventLog.HasConnectionEstablished);
        Assert.True(_eventLog.HasConnectionClosed);
        Assert.Contains(testMessage, _eventLog.DataReceived);

        // Check file was written
        if (File.Exists(_tempPath))
        {
            var fileContent = await File.ReadAllTextAsync(_tempPath);
            Assert.Equal(testMessage, fileContent);
        }
    }

    [Fact]
    public async Task StartTcpListener_EventsTest_EmitsCorrectEvents()
    {
        // Arrange
        var testPort = 16386; // Use a different port for this test
        var testMessage = "Test message for events";

        // Act - Start the listener and send a message
        var result = await StartBackgroundListener(testPort, () =>
            ConnectAndSendAsync(testPort, testMessage));

        // Assert events were fired
        Assert.True(_eventLog.HasConnectionEstablished);
        Assert.True(_eventLog.HasConnectionClosed);
        Assert.Contains(testMessage, _eventLog.DataReceived);
    }

    [Fact]
    public async Task StartTcpListener_ConnectWithoutData_EmitsConnectionEvents()
    {
        // Arrange
        var testPort = 16387;

        // Act - Start the listener and connect without sending data
        try
        {
            await StartBackgroundListener(testPort, () =>
                ConnectAndSendAsync(testPort));
        }
        catch (OperationCanceledException)
        {
            // Expected when no data is sent
        }

        // Assert connection events were fired but no data events
        Assert.True(_eventLog.HasConnectionEstablished);
        Assert.True(_eventLog.HasConnectionClosed);
        Assert.Empty(_eventLog.DataReceived);
    }

    [Fact]
    public async Task StartTcpListener_SingleMessage_ReceivesCorrectly()
    {
        // Arrange
        var testPort = 16388;
        var testMessage = "Single message";

        // Act - Start the listener and send a single message
        var result = await StartBackgroundListener(testPort, () =>
            ConnectAndSendAsync(testPort, testMessage));

        // Assert events and data
        Assert.True(_eventLog.HasConnectionEstablished);
        Assert.True(_eventLog.HasConnectionClosed);
        Assert.Single(_eventLog.DataReceived);
        Assert.Equal(testMessage, _eventLog.DataReceived[0]);
        Assert.Contains(testMessage, result);
    }

    [Fact]
    public async Task StartTcpListener_MultipleMessages_ReceivesAllCorrectly()
    {
        // Arrange
        var testPort = 16389;
        var messages = new[] { "Message 1", "Message 2", "Message 3" };

        // Act - Start the listener and send multiple messages
        var result = await StartBackgroundListener(testPort, () =>
            ConnectAndSendAsync(testPort, messages));

        // Assert events and data
        Assert.True(_eventLog.HasConnectionEstablished);
        Assert.True(_eventLog.HasConnectionClosed);

        // TCP may combine messages into fewer chunks, so verify total data received
        var allReceivedData = string.Join("", _eventLog.DataReceived);
        var expectedResult = string.Join("", messages);
        Assert.Equal(expectedResult, allReceivedData);
        Assert.Contains(expectedResult, result);
    }

    [Fact]
    public async Task StartTcpListener_NoConnection_TimesOutGracefully()
    {
        // Arrange
        var testPort = 16390;

        // Start the listener with short timeout
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        // Act & Assert - should handle timeout gracefully
        try
        {
            await _service.StartTcpListener(testPort, null, true, 30, 30, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when no connections come in within timeout
        }

        // No events should have been fired
        Assert.Empty(_eventLog.Events);
        Assert.Empty(_eventLog.DataReceived);
    }

    private NetworkServiceEventLog CreateEventLog()
    {
        var eventLog = new NetworkServiceEventLog();

        _service.ConnectionEstablished += eventLog.LogConnectionEstablished;
        _service.ConnectionClosed += eventLog.LogConnectionClosed;
        _service.DataReceived += eventLog.LogDataReceived;

        return eventLog;
    }

    private Task<string> StartBackgroundListener(int port, Func<Task> action) =>
        StartBackgroundListener(port, null, action);

    private async Task<string> StartBackgroundListener(int port, TimeSpan? timeout, Func<Task> action)
    {
        timeout ??= TimeSpan.FromSeconds(5);

        // Start the listener in background
        var cancellationTokenSource = new CancellationTokenSource(timeout.Value);
        var backgroundListener = Task.Run(() => _service.StartTcpListener(port, _tempPath, true, 30, 30, cancellationTokenSource.Token));

        // Give the listener a moment to start
        await Task.Delay(200);

        // Execute the action (send messages, connect, etc.)
        await action();

        // Wait for listener to complete
        return await backgroundListener;
    }

    private async Task ConnectAndSendAsync(int port, params string[] messages)
    {
        using var client = new TcpClient();

        await client.ConnectAsync("127.0.0.1", port);
        using var stream = client.GetStream();

        foreach (var message in messages)
        {
            var buffer = Encoding.ASCII.GetBytes(message);

            await stream.WriteAsync(buffer);
            await stream.FlushAsync();
        }
    }

    private class NetworkServiceEventLog
    {
        public List<string> Events { get; } = new();

        public List<string> DataReceived { get; } = new();

        public void LogConnectionEstablished(object? sender, ConnectionEventArgs e) =>
            Events.Add($"ConnectionEstablished:{e.RemoteEndPoint}");

        public void LogConnectionClosed(object? sender, ConnectionEventArgs e) =>
            Events.Add($"ConnectionClosed:{e.RemoteEndPoint}");

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
