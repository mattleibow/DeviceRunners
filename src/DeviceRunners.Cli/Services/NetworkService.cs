using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace DeviceRunners.Cli.Services;

public class NetworkService
{
    public bool IsPortAvailable(int port)
    {
        try
        {
            var tcpClient = new TcpClient();
            var result = tcpClient.BeginConnect(IPAddress.Loopback, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
            
            if (success)
            {
                tcpClient.EndConnect(result);
                tcpClient.Close();
                return false; // Port is in use
            }
            
            tcpClient.Close();
            return true; // Port is available
        }
        catch
        {
            return true; // Assume available if we can't test
        }
    }

    public async Task<string> StartTcpListener(int port, string? outputPath, bool nonInteractive, int connectionTimeoutSeconds = 30, int dataTimeoutSeconds = 30, CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        try
        {
            var receivedData = new StringBuilder();
            
            // Create connection timeout source for non-interactive mode
            using var connectionTimeoutSource = nonInteractive 
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;
            
            if (connectionTimeoutSource != null)
            {
                connectionTimeoutSource.CancelAfter(TimeSpan.FromSeconds(connectionTimeoutSeconds));
            }
            
            var connectionToken = connectionTimeoutSource?.Token ?? cancellationToken;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(100, connectionToken);
                    continue;
                }

                // Connection established, cancel connection timeout
                connectionTimeoutSource?.Dispose();

                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                
                var buffer = new byte[1024];
                var connectionData = new StringBuilder();
                
                // Create data timeout source for non-interactive mode
                using var dataTimeoutSource = nonInteractive 
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                    : null;
                
                int bytesRead;
                while (true)
                {
                    // Set/reset data timeout for each read operation in non-interactive mode
                    if (dataTimeoutSource != null)
                    {
                        dataTimeoutSource.CancelAfter(TimeSpan.FromSeconds(dataTimeoutSeconds));
                    }
                    
                    var dataToken = dataTimeoutSource?.Token ?? cancellationToken;
                    
                    try
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, dataToken);
                        if (bytesRead == 0) break;
                        
                        var data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        connectionData.Append(data);
                        
                        // Data received, reset timeout for next read
                        // The timeout will be reset at the beginning of the next loop iteration
                    }
                    catch (OperationCanceledException) when (dataTimeoutSource?.Token.IsCancellationRequested == true)
                    {
                        // Data timeout occurred in non-interactive mode
                        break;
                    }
                }

                var receivedMessage = connectionData.ToString();
                receivedData.AppendLine(receivedMessage);

                // Skip "ping" messages
                if (receivedMessage.Trim() != "ping")
                {
                    if (!string.IsNullOrEmpty(outputPath))
                    {
                        await File.WriteAllTextAsync(outputPath, receivedMessage, cancellationToken);
                    }

                    if (nonInteractive)
                    {
                        break;
                    }
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