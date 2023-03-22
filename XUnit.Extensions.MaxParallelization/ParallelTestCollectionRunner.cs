using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;
using XUnit.Extensions.MaxParallelization.DI;

namespace XUnit.Extensions.MaxParallelization;
public class ParallelTestCollectionRunner : XunitTestCollectionRunner
{
    protected FixtureContainer AssemblyFixtureContainer;
    protected FixtureContainer CollectionContainer;

    private readonly SemaphoreSlim _testCollectionsSemaphore;
    private readonly SemaphoreSlim _testClassesSemaphore;
    private readonly SemaphoreSlim _testCasesSemaphore;

    private FixtureRegistrationCollection fixtureRegistrations;
    public ParallelTestCollectionRunner(
        ITestCollection testCollection, 
        IEnumerable<IXunitTestCase> testCases, 
        IMessageSink diagnosticMessageSink, 
        IMessageBus messageBus, 
        ITestCaseOrderer testCaseOrderer, 
        ExceptionAggregator aggregator, 
        CancellationTokenSource cancellationTokenSource,
        FixtureRegistrationCollection fixtureRegistrations,
        FixtureContainer assemblyFixtureContainer,
        SemaphoreSlim collectionSemaphore,
        SemaphoreSlim classSemaphore,
        SemaphoreSlim testCasesSemaphore) : base(
            testCollection, 
            testCases, 
            diagnosticMessageSink, 
            messageBus, 
            testCaseOrderer, 
            aggregator, 
            cancellationTokenSource)
    {
        AssemblyFixtureContainer = assemblyFixtureContainer;
        this._testCollectionsSemaphore = collectionSemaphore;
        this._testClassesSemaphore = classSemaphore;
        this._testCasesSemaphore = testCasesSemaphore;
        this.fixtureRegistrations = fixtureRegistrations;
    }

    public async Task<RunSummary> RunTestsAsync()
    {
        await _testCollectionsSemaphore.WaitAsync();
        try
        {
            var collectionSummary = new RunSummary();

            if (!MessageBus.QueueMessage(new TestCollectionStarting(TestCases.Cast<ITestCase>(), TestCollection)))
                CancellationTokenSource.Cancel();
            else
            {
                try
                {
                    await AfterTestCollectionStartingAsync();
                    collectionSummary = await RunTestClassesAsync();

                    Aggregator.Clear();
                    await BeforeTestCollectionFinishedAsync();

                    if (Aggregator.HasExceptions)
                        if (!MessageBus.QueueMessage(new TestCollectionCleanupFailure(TestCases.Cast<ITestCase>(), TestCollection, Aggregator.ToException())))
                            CancellationTokenSource.Cancel();
                }
                finally
                {
                    if (!MessageBus.QueueMessage(new TestCollectionFinished(TestCases.Cast<ITestCase>(), TestCollection, collectionSummary.Time, collectionSummary.Total, collectionSummary.Failed, collectionSummary.Skipped)))
                        CancellationTokenSource.Cancel();
                }
            }

            return collectionSummary;
        }
        finally 
        { 
            _testCollectionsSemaphore.Release();
        }
    }

    protected override async Task AfterTestCollectionStartingAsync()
    {
        var containerBuilder = new FixtureContainerBuilder();
        CollectionContainer = containerBuilder.BuildContainer(fixtureRegistrations, FixtureRegisterationLevel.Collection,AssemblyFixtureContainer);
        await CollectionContainer.InitializeAsync();
        await base.AfterTestCollectionStartingAsync();
    }

    protected override async Task BeforeTestCollectionFinishedAsync()
    {
        await CollectionContainer.DisposeAsync();
        await base.BeforeTestCollectionFinishedAsync();
    }

    protected async override Task<RunSummary> RunTestClassesAsync()             
    {
        if (TestCollection.IsParallelizationDisabled())
            return await base.RunTestClassesAsync();
        var groups = TestCases
            .GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance);

        var summary = new RunSummary();

        var testClassesTasks = groups.Select(testCasesByClass => RunTestClassAsync(
                    testCasesByClass.Key,
                    (IReflectionTypeInfo)testCasesByClass.Key.Class,
                    testCasesByClass));

        var classSummaries = await Task.WhenAll(testClassesTasks).ConfigureAwait(false); ;

        foreach (var classSummary in classSummaries)
        {
            summary.Aggregate(classSummary);
        }

        return summary;
    }
    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
    {

        var mergedContainer = FixtureContainerMerger.Merge(AssemblyFixtureContainer, CollectionContainer);
        return new ParallelTestClassRunner(
            testClass, 
            @class, 
            testCases, 
            DiagnosticMessageSink, 
            MessageBus, 
            TestCaseOrderer, 
            new ExceptionAggregator(Aggregator), 
            CancellationTokenSource,
            fixtureRegistrations,
            mergedContainer,
            _testClassesSemaphore,
            _testCasesSemaphore).RunTestsAsync();
    }
}
