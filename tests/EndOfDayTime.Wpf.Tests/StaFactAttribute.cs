using System;
using System.Threading;
using Xunit.Sdk;
using Xunit.Abstractions;
using System.Collections.Generic;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace EndOfDayTime.Wpf.Tests
{
    /// <summary>
    /// Runs a test on an STA thread, required for WPF controls.
    /// Usage: [StaFact] instead of [Fact]
    /// </summary>
    public class StaFactAttribute : Xunit.FactAttribute { }

    public class StaTestCase : XunitTestCase
    {
        [Obsolete("For serialization only", true)]
        public StaTestCase() { }

        public StaTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod,
            object[]? testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        { }

        public override System.Threading.Tasks.Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<RunSummary>();
            var thread = new Thread(() =>
            {
                try
                {
                    var result = base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource).GetAwaiter().GetResult();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}