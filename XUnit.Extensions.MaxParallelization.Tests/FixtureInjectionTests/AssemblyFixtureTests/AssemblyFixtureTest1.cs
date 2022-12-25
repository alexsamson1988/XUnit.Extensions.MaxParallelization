﻿using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.FixtureInjectionTests.AssemblyFixtureTests;
public class AssemblyFixtureTest1
{
    private readonly AssemblyParallelTestFixture assemblyParallelTestFixture;

    public AssemblyFixtureTest1(AssemblyParallelTestFixture assemblyParallelTestFixture)
    {
        this.assemblyParallelTestFixture = assemblyParallelTestFixture;
    }

    [Fact]
    public void TestIncrement1()
    {
        var expected = 2;
        var actual = assemblyParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }


}
