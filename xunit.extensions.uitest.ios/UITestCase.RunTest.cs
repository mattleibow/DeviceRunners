using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest
{
    partial class UITestCase
    {
        void RunTestImpl(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, TaskCompletionSource<RunSummary> tcs)
        {
            // Run on the UI thread
             NSRunLoop.Main.BeginInvokeOnMainThread(
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
        }
    }
}
