using System.Net;

namespace DeviceRunners.Cli.Services;

public class ConnectionEventArgs : EventArgs
{
    public IPEndPoint? RemoteEndPoint { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
}

public class DataReceivedEventArgs : EventArgs
{
    public string Data { get; init; } = string.Empty;
    public IPEndPoint? RemoteEndPoint { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
}