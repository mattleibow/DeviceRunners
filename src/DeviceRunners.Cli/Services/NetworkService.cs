using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeviceRunners.Cli.Services;

public class NetworkService
{
    public event EventHandler<ConnectionEventArgs>? ConnectionEstablished;

    public event EventHandler<ConnectionEventArgs>? ConnectionClosed;

    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    public bool IsPortAvailable(int port)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var result = tcpClient.BeginConnect(IPAddress.Loopback, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));

            if (success)
            {
                tcpClient.EndConnect(result);
                return false; // Port is in use
            }

            return true; // Port is available
        }
        catch
        {
            return true; // Assume available if we can't test
        }
    }

    public async Task<string> StartTcpListener(int port, string? outputPath, bool nonInteractive, int? connectionTimeoutSeconds = null, int? dataTimeoutSeconds = null, CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        try
        {
            var receivedData = new StringBuilder();

            // Create connection timeout only for non-interactive mode
            using var connectionTimeoutSource = nonInteractive
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;

            if (connectionTimeoutSource is not null && connectionTimeoutSeconds is int connTimeout)
            {
                connectionTimeoutSource.CancelAfter(TimeSpan.FromSeconds(connTimeout));
            }

            var connectionToken = connectionTimeoutSource?.Token ?? cancellationToken;

            // Wait for first connection with timeout (if non-interactive)
            while (!connectionToken.IsCancellationRequested)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(100, connectionToken);
                    continue;
                }

                using var client = await listener.AcceptTcpClientAsync();
                var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

                // Emit connection established event
                ConnectionEstablished?.Invoke(this, new ConnectionEventArgs
                {
                    RemoteEndPoint = remoteEndPoint
                });

                using var stream = client.GetStream();

                var buffer = new byte[1024];
                var connectionData = new StringBuilder();

                // Create data timeout for this connection only in non-interactive mode
                using var dataTimeoutSource = nonInteractive
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                    : null;

                // Set initial data timeout before first read (if non-interactive)
                if (dataTimeoutSource is not null && dataTimeoutSeconds is int dataTimeout)
                {
                    dataTimeoutSource.CancelAfter(TimeSpan.FromSeconds(dataTimeout));
                }

                var dataToken = dataTimeoutSource?.Token ?? cancellationToken;

                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, dataToken)) > 0)
                {
                    var data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    connectionData.Append(data);

                    // Emit data received event for each chunk
                    DataReceived?.Invoke(this, new DataReceivedEventArgs
                    {
                        Data = data,
                        RemoteEndPoint = remoteEndPoint
                    });

                    // Reset data timeout on each data received (if non-interactive)
                    if (dataTimeoutSource is not null && dataTimeoutSeconds is int dataAgainTimeout)
                    {
                        dataTimeoutSource.CancelAfter(TimeSpan.FromSeconds(dataAgainTimeout));
                    }
                }

                // Emit connection closed event
                ConnectionClosed?.Invoke(this, new ConnectionEventArgs
                {
                    RemoteEndPoint = remoteEndPoint
                });

                // Add the received message to the output
                var receivedMessage = connectionData.ToString();
                receivedData.AppendLine(receivedMessage);

                // Skip "ping" messages
                if (receivedMessage.Trim() == "ping")
                {
                    continue;
                }

                // If an output path is specified, write the received message to the file
                if (!string.IsNullOrEmpty(outputPath))
                {
                    var dir = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir!);
                    }
                    await File.WriteAllTextAsync(outputPath, receivedMessage, cancellationToken);
                }

                if (nonInteractive)
                {
                    break;
                }
            }

            return receivedData.ToString();
        }
        finally
        {
            listener.Stop();
        }
    }
}
