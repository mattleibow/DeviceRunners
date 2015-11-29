using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest
{
    class UITestCase : IXunitTestCase
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

            //var disp = SecondaryView.Dispatcher;
            var disp = CoreApplication.MainView.Dispatcher;

            var tcs = new TaskCompletionSource<RunSummary>();



#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                              // Run on the current window's dispatcher
            disp.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    try
                    {
                        var result = testCase.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
                        result.ContinueWith(t =>
                                            {
                                                if (t.IsFaulted)
                                                    tcs.SetException(t.Exception);

                                                tcs.SetResult(t.Result);
                                            });
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }

                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return tcs.Task;
        }

        public IMethodInfo Method => testCase.Method;
    }
}
