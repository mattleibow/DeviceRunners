using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.Runner.Services
{
    public class TcpWriterInfo : IEquatable<TcpWriterInfo>
    {
        public TcpWriterInfo(string hostName, int port)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentNullException(nameof(hostName));
            }

            if ((port < 0) || (port > ushort.MaxValue))
            {
                throw new ArgumentException(nameof(port));
            }

            this.Hostname = hostName;
            this.Port = port;
        }

        public string Hostname { get; set; }

        public int Port { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(TcpWriterInfo other)
        {
            return Hostname.Equals(other.Hostname, StringComparison.OrdinalIgnoreCase) && Port == other.Port;
        }

        public override string ToString()
        {
            return $"{Hostname}:{Port}";
        }
    }
}
