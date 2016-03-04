// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;

using System.Threading.Tasks;

#if NETFX_CORE
using Windows.Networking;
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
#endif

namespace NUnit.Runner.Services
{
    /// <summary>
    /// Redirects output to a Tcp connection
    /// </summary>
    class TcpWriter : TextWriter
    {
        private readonly string hostName;
        private readonly int port;
        
        private StreamWriter writer;

        public TcpWriter(string hostName, int port)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentNullException(nameof(hostName));
            }

            if ((port < 0) || (port > ushort.MaxValue))
            {
                throw new ArgumentException(nameof(port));
            }

            this.hostName = hostName;
            this.port = port;
        }

        public async Task Connect()
        {
#if NETFX_CORE
            var socket = new StreamSocket();
            await
                socket.ConnectAsync(new HostName(hostName), port.ToString())
                      .AsTask()
                      .ContinueWith(@object => writer = new StreamWriter(socket.OutputStream.AsStreamForWrite()));
#else
            TcpClient client;
            NetworkStream stream;
            await Task.Run(
                () =>
                {
                    client = new TcpClient(hostName, port);
                    stream = client.GetStream();
                    this.writer = new StreamWriter(stream);
                }).ConfigureAwait(false);
#endif
        }

        public override void Write(char value)
        {
            writer.Write(value);
        }

        public override void Write(string value)
        {
            writer.Write(value);
        }

        public override void WriteLine(string value)
        {
            writer.WriteLine(value);
            writer.Flush();
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        protected override void Dispose(bool disposing)
        {
            writer?.Dispose();
        }
    }
}