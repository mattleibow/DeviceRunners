using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework.Interfaces;

namespace NUnit.Runner.Services
{
    public abstract class TestResultProcessor
    {
        protected TestResultProcessor(TestOptions options)
        {
            Options = options;
        }

        protected TestOptions Options { get; private set; }

        public TestResultProcessor Successor { get; set; }

        public abstract Task Process(ITestResult testResult);

        public static TestResultProcessor BuildChainOfResponsability(TestOptions options)
        {
            var tcpWriter = new TcpWriterProcessor(options);
            var xmlFileWriter = new XmlFileProcessor(options);

            tcpWriter.Successor = xmlFileWriter;
            return tcpWriter;
        }
    }
}
