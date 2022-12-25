using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.FixtureInjectionTests.MethodFixtureTests;
public class MethodFixtureTests
{
    private readonly MethodParallelTestFixture methodParallelTestFixture;

    public MethodFixtureTests(MethodParallelTestFixture methodParallelTestFixture)
	{
        this.methodParallelTestFixture = methodParallelTestFixture;
    }
    [Fact]
    public void Increment1_Should_Return_1_1()
    {
        var expected = 1;
        var actual = this.methodParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Increment1_Should_Return_1_2()
    {
        var expected = 1;
        var actual = this.methodParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

}
