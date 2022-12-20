using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.FixtureInjectionTests.ClassFixtureTests;
public class ClassFixtureTest2
{
    private readonly ClassParallelTestFixture classParallelTestFixture;

    public ClassFixtureTest2(ClassParallelTestFixture classParallelTestFixture)
    {
        this.classParallelTestFixture = classParallelTestFixture;
    }

    [Fact]
    public void TestIncrement1()
    {
        var expected = 3;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestIncrement2()
    {
        var expected = 3;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestIncrement3()
    {
        var expected = 3;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }


}
