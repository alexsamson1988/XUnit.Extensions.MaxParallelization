using Xunit.Abstractions;
using Xunit.Sdk;

namespace XUnit.Extensions.MaxParallelization;
public class ParallelTestMethodRunner : XunitTestMethodRunner
{
    readonly object[] constructorArguments;
    readonly IMessageSink diagnosticMessageSink;

    public ParallelTestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments)
    {
        this.constructorArguments = constructorArguments;
        this.diagnosticMessageSink = diagnosticMessageSink;
    }

    protected override void AfterTestMethodStarting()
    {

        base.AfterTestMethodStarting();
    }

    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        if (TestMethod.IsParallelizationDisabled())
            return await base.RunTestCasesAsync().ConfigureAwait(false);

        var summary = new RunSummary();

        var caseTasks = TestCases.Select(RunTestCaseAsync);
        var caseSummaries = await Task.WhenAll(caseTasks).ConfigureAwait(false);

        foreach (var caseSummary in caseSummaries)
        {
            summary.Aggregate(caseSummary);
        }

        return summary;
    }

    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        var args = constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();

        var action = () => testCase.RunAsync(diagnosticMessageSink, MessageBus, args, new ExceptionAggregator(Aggregator), CancellationTokenSource);

        if (SynchronizationContext.Current != null)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return await Task.Factory.StartNew(action, CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap().ConfigureAwait(false);
        }

        return await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
    }
}
