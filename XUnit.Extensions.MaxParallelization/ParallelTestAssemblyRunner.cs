using System.Reflection;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using XUnit.Extensions.MaxParallelization.DI;

namespace XUnit.Extensions.MaxParallelization;
public class ParallelTestAssemblyRunner : XunitTestAssemblyRunner
{
    protected FixtureContainer AssemblyContainer;

    private FixtureRegistrationCollection fixtureRegistrationCollection;
    public ParallelTestAssemblyRunner(
        ITestAssembly testAssembly,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions){ }
    protected override async Task AfterTestAssemblyStartingAsync()
    {
        fixtureRegistrationCollection = TestAssembly.ResolveFixtureRegistrations();
        await base.AfterTestAssemblyStartingAsync();
        await CreateAssemlbyContainerAsync();
    }

    protected override async Task BeforeTestAssemblyFinishedAsync()
    {
        await AssemblyContainer.DisposeAsync();

        await base.BeforeTestAssemblyFinishedAsync();
    }

    protected virtual async Task CreateAssemlbyContainerAsync()
    {
        var containerBuilder = new FixtureContainerBuilder();
        AssemblyContainer = containerBuilder.BuildContainer(fixtureRegistrationCollection, FixtureRegisterationLevel.Assembly);
        await AssemblyContainer.InitializeAsync();
    }

    protected override async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus, CancellationTokenSource cancellationTokenSource)
    {
        if (TestAssembly.IsParallelizationDisabled())
            return await base.RunTestCollectionsAsync(messageBus, cancellationTokenSource);
        
        var summary = new RunSummary();

        var collectionTasks = OrderTestCollections().Select(collection => RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource));

        var summaries = await Task.WhenAll(collectionTasks);

        foreach (var collectionSummary in summaries)
        {
            summary.Aggregate(collectionSummary);
        }

        return summary;
    }

    protected override Task<RunSummary> RunTestCollectionAsync(
        IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource)
    {
        return
            new ParallelTestCollectionRunner(
                testCollection, 
                testCases, 
                DiagnosticMessageSink, 
                messageBus, 
                TestCaseOrderer, 
                new ExceptionAggregator(Aggregator), 
                cancellationTokenSource,
                fixtureRegistrationCollection,
                AssemblyContainer).RunAsync();
    }
}

