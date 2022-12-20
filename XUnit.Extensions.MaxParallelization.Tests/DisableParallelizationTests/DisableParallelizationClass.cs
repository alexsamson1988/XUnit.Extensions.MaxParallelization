using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.DisableParallelizationTests;
[DisableParallelization]
public class DisableParallelizationClass
{
    private readonly ClassParallelTestFixture classParallelTestFixture;

    public DisableParallelizationClass(ClassParallelTestFixture classParallelTestFixture)
    {
        this.classParallelTestFixture = classParallelTestFixture;
    }

    [Fact]
    public void TestParallel1()
    {
        var expected = 1;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestParallel2()
    {
        var expected = 2;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestParallel3()
    {
        var expected = 3;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }
}
