using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest
{
    partial class UITestCase : IXunitTestCase
    {
        IXunitTestCase testCase;

        public UITestCase(IXunitTestCase testCase)
        {
            this.testCase = testCase;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the deserializer", true)]
        public UITestCase()
        {
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            testCase = info.GetValue<IXunitTestCase>("InnerTestCase");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("InnerTestCase", testCase);
        }

        public string DisplayName => testCase.DisplayName;
        public string SkipReason => testCase.SkipReason;

        public ISourceInformation SourceInformation
        {
            get { return testCase.SourceInformation; }
            set { testCase.SourceInformation = value; }
        }

        public ITestMethod TestMethod => testCase.TestMethod;
        public object[] TestMethodArguments => testCase.TestMethodArguments;
        public Dictionary<string, List<string>> Traits => testCase.Traits;
        public string UniqueID => testCase.UniqueID;


        public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            var tcs = new TaskCompletionSource<RunSummary>();

            RunTestImpl(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource, tcs);

            return tcs.Task;
        }

        public IMethodInfo Method => testCase.Method;
    }
}
