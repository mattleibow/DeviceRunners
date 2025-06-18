using System.Net;

namespace DeviceRunners.Cli.Services;

public class ConnectionEventArgs : EventArgs
{
    public IPEndPoint? RemoteEndPoint { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}

public class DataReceivedEventArgs : EventArgs
{
    public string Data { get; init; } = string.Empty;

    public IPEndPoint? RemoteEndPoint { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}