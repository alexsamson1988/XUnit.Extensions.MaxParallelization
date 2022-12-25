﻿using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.FixtureInjectionTests.AssemblyFixtureTests;
public class AssemblyFixtureTest2
{
    private readonly AssemblyParallelTestFixture assemblyParallelTestFixture;

    public AssemblyFixtureTest2(AssemblyParallelTestFixture assemblyParallelTestFixture)
    {
        this.assemblyParallelTestFixture = assemblyParallelTestFixture;
    }

    [Fact]
    public void TestIncrement1()
    {
        var expected = 6;
        var actual = assemblyParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestIncrement2()
    {
        var expected = 6;
        var actual = assemblyParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestIncrement3()
    {
        var expected = 6;
        var actual = assemblyParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }
}