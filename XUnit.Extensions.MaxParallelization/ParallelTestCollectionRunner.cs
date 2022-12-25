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
        FixtureContainer assemblyFixtureContainer) : base(
            testCollection, 
            testCases, 
            diagnosticMessageSink, 
            messageBus, 
            testCaseOrderer, 
            aggregator, 
            cancellationTokenSource)
    {
        AssemblyFixtureContainer = assemblyFixtureContainer;
        this.fixtureRegistrations = fixtureRegistrations;
    }

    protected override async Task AfterTestCollectionStartingAsync()
    {
        var containerBuilder = new FixtureContainerBuilder();
        CollectionContainer = containerBuilder.BuildContainer(fixtureRegistrations, FixtureRegisterationLevel.Collection);
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

        var classSummaries = await Task.WhenAll(testClassesTasks);

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
            mergedContainer).RunAsync();
    }
}
