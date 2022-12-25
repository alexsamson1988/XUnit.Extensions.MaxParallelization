using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;
using XUnit.Extensions.MaxParallelization.DI;

namespace XUnit.Extensions.MaxParallelization;
public class ParallelTestMethodRunner : XunitTestMethodRunner
{
    readonly object[] constructorArguments;
    private readonly FixtureRegistrationCollection fixtureRegistrations;
    private readonly FixtureContainer classContainer;
    readonly IMessageSink diagnosticMessageSink;
    private FixtureContainer methodContainer;
    private Dictionary<Type, object> MethodFixtureMapping;
    private IReflectionTypeInfo classInfos;
    public ParallelTestMethodRunner(
        ITestMethod testMethod, 
        IReflectionTypeInfo @class, 
        IReflectionMethodInfo method, 
        IEnumerable<IXunitTestCase> testCases, 
        IMessageSink diagnosticMessageSink, 
        IMessageBus messageBus, 
        ExceptionAggregator aggregator, 
        CancellationTokenSource cancellationTokenSource,
        FixtureContainer classContainer,
        FixtureRegistrationCollection fixtureRegistrations)
        : base(
            testMethod, 
            @class, 
            method, 
            testCases, 
            diagnosticMessageSink, 
            messageBus, 
            aggregator, 
            cancellationTokenSource, 
            null)
    {
        this.fixtureRegistrations = fixtureRegistrations;
        this.classContainer = classContainer;
        this.diagnosticMessageSink = diagnosticMessageSink;
        this.classInfos = @class;
    }

    

    protected async Task BeforeTestMethodFinishedAsync()
    {
        await this.methodContainer.DisposeAsync();
        base.BeforeTestMethodFinished();
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

    private async Task<object[]> BuildConstructorArgumentsAsync(IXunitTestCase testCase)
    {
        var containerBuilder = new FixtureContainerBuilder();
        var methodLevelContainer = containerBuilder.BuildContainer(fixtureRegistrations, FixtureRegisterationLevel.Method);
        await methodLevelContainer.InitializeAsync();

        methodContainer = FixtureContainerMerger.Merge(methodLevelContainer, classContainer);
        var mergedMethodContainer = FixtureContainerMerger.Merge(classContainer, methodContainer);

        return classInfos.CreateTestClassConstructorArguments(methodContainer, Aggregator);
    }

    

    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        var args = await BuildConstructorArgumentsAsync(testCase);

        var action = () => testCase.RunAsync(diagnosticMessageSink, MessageBus, args, new ExceptionAggregator(Aggregator), CancellationTokenSource);

        if (SynchronizationContext.Current != null)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return await Task.Factory.StartNew(action, CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap().ConfigureAwait(false);
        }
        await BeforeTestMethodFinishedAsync();
        return await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
    }
}
