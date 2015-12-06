using System;
using System.Collections.Generic;
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
    partial class UITestCase
    {
        void RunTestImpl(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, TaskCompletionSource<RunSummary> tcs)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var disp = CoreApplication.MainView.Dispatcher;

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
                                  tcs.TrySetException(e);
                              }
                          }
                );

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
