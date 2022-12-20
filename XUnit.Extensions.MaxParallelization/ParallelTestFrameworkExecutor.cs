﻿using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace XUnit.Extensions.MaxParallelization;
/// <summary>
/// Xunit.Extensions.Ordering test framework executor.
/// </summary>
public class ParallelTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public ParallelTestFrameworkExecutor(
        AssemblyName assemblyName,
        ISourceInformationProvider sourceInformationProvider,
        IMessageSink diagnosticMessageSink)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink) { }

    protected override async void RunTestCases(
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        using (var assemblyRunner =
            new ParallelTestAssemblyRunner(
                TestAssembly,
                testCases,
                DiagnosticMessageSink,
                executionMessageSink,
                executionOptions))
            await assemblyRunner.RunAsync();
    }
}
