using System;
using System.Diagnostics;
using System.Threading.Tasks;

using NUnit.Framework.Interfaces;

namespace NUnit.Runner.Services
{
    public class TcpWriterProcessor : TestResultProcessor
    {
        public TcpWriterProcessor(TestOptions options)
            : base(options)
        {
        }

        public override async Task Process(ITestResult testResult)
        {
            if (Options.TcpWriterParamaters != null)
            {
                try
                {
                    await WriteResult(testResult);
                }
                catch (Exception exception)
                {
                    string message = $"Fatal error while trying to send xml result by TCP to {Options.TcpWriterParamaters}\nDoes your server is running ?";
                    throw new InvalidOperationException(message, exception);
                }
            }

            if (Successor != null)
            {
                await Successor.Process(testResult);
            }
        }

        private async Task WriteResult(ITestResult testResult)
        {
            using (var tcpWriter = new TcpWriter(Options.TcpWriterParamaters.Hostname, Options.TcpWriterParamaters.Port))
            {
                await tcpWriter.Connect().ConfigureAwait(false);
                tcpWriter.Write(testResult.ToXml(true).OuterXml);
            }
        }
    }
}