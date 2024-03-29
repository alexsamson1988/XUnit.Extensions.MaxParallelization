﻿using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;
using XUnit.Extensions.MaxParallelization.DI;

namespace XUnit.Extensions.MaxParallelization;
public class ParallelTestClassRunner : XunitTestClassRunner
{
    private readonly FixtureRegistrationCollection _fixtureRegistrations;
    private readonly FixtureContainer _collectionContainer;
    private FixtureContainer _classContainer;
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
        this._fixtureRegistrations = fixtureRegistrations;
        this._collectionContainer = collectionContainer;
    }
    protected override async Task AfterTestClassStartingAsync()
    {
        var containerBuilder = new FixtureContainerBuilder();
        var classLevelContainer = containerBuilder.BuildContainer(_fixtureRegistrations, FixtureRegisterationLevel.Class,_collectionContainer);
        await classLevelContainer.InitializeAsync();
        ClassFixtureMappings = classLevelContainer.Fixtures;
        _classContainer = FixtureContainerMerger.Merge(classLevelContainer, _collectionContainer);
        await base.AfterTestClassStartingAsync();
    }

    protected override async Task BeforeTestClassFinishedAsync()
    {
        await this._classContainer.DisposeAsync();
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

        await RunParallelTestCasesAsync(parallelTests, summary);
        await RunSequentialTestCasesAsync(sequentialsTests, summary);

        return summary;
    }

    private async Task RunSequentialTestCasesAsync(IEnumerable<IXunitTestCase> sequentialsTestCases, RunSummary summary)
    {
        var sequentialMethodGroups = sequentialsTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance).ToList();

        foreach (var sequentialTestGroup in sequentialMethodGroups)
        {
            summary.Aggregate(await RunTestMethodAsync(sequentialTestGroup.Key, (IReflectionMethodInfo)sequentialTestGroup.Key.Method, sequentialTestGroup));
        }
    }

    private async Task RunParallelTestCasesAsync(IEnumerable<IXunitTestCase> parallelTestCases, RunSummary summary)
    {
        var methodGroups = parallelTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance).ToList();
        var methodTasks = methodGroups.Select(m => RunTestMethodAsync(m.Key, (IReflectionMethodInfo)m.Key.Method, m));
        var summaries = await Task.WhenAll(methodTasks).ConfigureAwait(false);
        foreach (var currentSummary in summaries)
        {
            summary.Aggregate(currentSummary);
        }
    }

    protected async Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases)
        => await new ParallelTestMethodRunner(
            testMethod, 
            Class,
            method, 
            testCases, 
            DiagnosticMessageSink, 
            MessageBus, 
            new ExceptionAggregator(Aggregator), 
            CancellationTokenSource,
            _classContainer,
            _fixtureRegistrations).RunAsync();

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
