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

    public async Task<string> StartTcpListener(int port, string? outputPath, bool nonInteractive, CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        try
        {
            var receivedData = new StringBuilder();
            
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                
                var buffer = new byte[1024];
                var connectionData = new StringBuilder();
                
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    var data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    connectionData.Append(data);
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