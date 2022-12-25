using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.FixtureInjectionTests.ClassFixtureTests;
public class ClassFixtureTest1
{
    private readonly ClassParallelTestFixture classParallelTestFixture;

    public ClassFixtureTest1(ClassParallelTestFixture classParallelTestFixture)
    {
        this.classParallelTestFixture = classParallelTestFixture;
    }

    [Fact]
    public void TestIncrement1()
    {
        var expected = 2;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestIncrement2()
    {
        var expected = 2;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }



}
