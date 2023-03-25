using Xunit.Abstractions;
using Xunit.Sdk;
using XUnit.Extensions.MaxParallelization.DI;

namespace XUnit.Extensions.MaxParallelization;
public class ParallelTestMethodRunner : XunitTestMethodRunner
{
    readonly object[] _constructorArguments;
    private readonly FixtureRegistrationCollection _fixtureRegistrations;
    private readonly FixtureContainer _classContainer;
    readonly IMessageSink _diagnosticMessageSink;
    private FixtureContainer _methodContainer;
    private Dictionary<Type, object> _MethodFixtureMapping;
    private IReflectionTypeInfo _classInfos;
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
        this._fixtureRegistrations = fixtureRegistrations;
        this._classContainer = classContainer;
        this._diagnosticMessageSink = diagnosticMessageSink;
        this._classInfos = @class;
    }

    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        if (TestMethod.IsParallelizationDisabled())
            return await base.RunTestCasesAsync().ConfigureAwait(false);

        var summary = new RunSummary();

        var summaries = await Task.WhenAll(TestCases.Select(RunTestCaseAsync)).ConfigureAwait(false);

        foreach (var currentSummary in summaries)
        {
            summary.Aggregate(currentSummary);
        }

        return summary;
    }

    private async Task<object[]> BuildConstructorArgumentsAsync(IXunitTestCase testCase)
    {
        var containerBuilder = new FixtureContainerBuilder();
        var methodLevelContainer = containerBuilder.BuildContainer(_fixtureRegistrations, FixtureRegisterationLevel.Method, _classContainer);
        await methodLevelContainer.InitializeAsync();

        _methodContainer = FixtureContainerMerger.Merge(methodLevelContainer, _classContainer);

        return _classInfos.CreateTestClassConstructorArguments(_methodContainer, Aggregator);
    }

    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        var args = await BuildConstructorArgumentsAsync(testCase);

        var action = () => testCase.RunAsync(_diagnosticMessageSink, MessageBus, args, new ExceptionAggregator(Aggregator), CancellationTokenSource);

        if (SynchronizationContext.Current != null)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return await Task.Factory.StartNew(action, CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap().ConfigureAwait(false);
        }

        return await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
    }
}
