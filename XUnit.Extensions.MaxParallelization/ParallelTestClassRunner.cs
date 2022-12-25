using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using XUnit.Extensions.MaxParallelization.DI;

namespace XUnit.Extensions.MaxParallelization;
public class ParallelTestClassRunner : XunitTestClassRunner
{
    private readonly FixtureRegistrationCollection fixtureRegistrations;
    private readonly FixtureContainer collectionContainer;
    private FixtureContainer classContainer;
    public ParallelTestClassRunner(
        ITestClass testClass, 
        IReflectionTypeInfo @class, 
        IEnumerable<IXunitTestCase> testCases, 
        IMessageSink diagnosticMessageSink, 
        IMessageBus messageBus, 
        ITestCaseOrderer testCaseOrderer, 
        ExceptionAggregator aggregator, 
        CancellationTokenSource cancellationTokenSource,
        FixtureRegistrationCollection fixtureRegistrations,
        FixtureContainer collectionContainer) : base(
            testClass, 
            @class, 
            testCases, 
            diagnosticMessageSink, 
            messageBus, 
            testCaseOrderer, 
            aggregator, 
            cancellationTokenSource, 
            null)
    {
        this.fixtureRegistrations = fixtureRegistrations;
        this.collectionContainer = collectionContainer;
    }
    protected override async Task AfterTestClassStartingAsync()
    {
        var containerBuilder = new FixtureContainerBuilder();
        var classLevelContainer = containerBuilder.BuildContainer(fixtureRegistrations, FixtureRegisterationLevel.Class);
        await classLevelContainer.InitializeAsync();
        ClassFixtureMappings = classLevelContainer.Fixtures;
        classContainer = FixtureContainerMerger.Merge(classLevelContainer, collectionContainer);
        await base.AfterTestClassStartingAsync();
    }

    protected override async Task BeforeTestClassFinishedAsync()
    {
        await this.classContainer.DisposeAsync();
        await base.BeforeTestClassFinishedAsync();
    }
    protected override async Task<RunSummary> RunTestMethodsAsync()
    {
         
        var summary = new RunSummary();
        IEnumerable<IXunitTestCase> orderedTestCases;
        try
        {
            orderedTestCases = TestCaseOrderer.OrderTestCases(TestCases);
        }
        catch (Exception ex)
        {
            var innerEx = Unwrap(ex);
            DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Test case orderer '{TestCaseOrderer.GetType().FullName}' threw '{innerEx.GetType().FullName}' during ordering: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));
            orderedTestCases = TestCases.ToList();
        }

        var sequentialsTests = orderedTestCases.Where(testCase => testCase.TestMethod.IsParallelizationDisabled());
        var parallelTests = orderedTestCases.Where(testCase => !testCase.TestMethod.IsParallelizationDisabled());

        var methodGroups = parallelTests.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance);
        var methodTasks = methodGroups.Select(m => RunTestMethodAsync(m.Key, (IReflectionMethodInfo)m.Key.Method, m));
        var methodSummaries = await Task.WhenAll(methodTasks).ConfigureAwait(false);

        foreach (var methodSummary in methodSummaries)
        {
            summary.Aggregate(methodSummary);
        } 

        var sequentialMethodGroups = sequentialsTests.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance).ToList();

        foreach (var sequentialTestGroup in sequentialMethodGroups)
        {
            summary.Aggregate(await RunTestMethodAsync(sequentialTestGroup.Key, (IReflectionMethodInfo)sequentialTestGroup.Key.Method, sequentialTestGroup));
        }

        return summary;
    }

    protected Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases)
        => new ParallelTestMethodRunner(
            testMethod, 
            Class, 
            method, 
            testCases, 
            DiagnosticMessageSink, 
            MessageBus, 
            new ExceptionAggregator(Aggregator), 
            CancellationTokenSource,
            classContainer,
            fixtureRegistrations).RunAsync();

    private static Exception Unwrap(Exception ex)
    {
        while (true)
        {
            if (ex is not TargetInvocationException tiex || tiex.InnerException == null)
                return ex;

            ex = tiex.InnerException;
        }
    }
}
