using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.ParallelizationTests;
public class TheoryParallelizationTests
{
    private readonly ClassParallelTestFixture classParallelTestFixture;

    public TheoryParallelizationTests(ClassParallelTestFixture classParallelTestFixture)
    {
        this.classParallelTestFixture = classParallelTestFixture;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void TestTheoryParallelization(int i)
    {
        var expected = 2;
        var actual = classParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }


}
