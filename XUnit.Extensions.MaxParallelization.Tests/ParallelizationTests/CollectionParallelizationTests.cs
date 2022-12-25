using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.ParallelizationTests;

[CollectionDefinition("ParallelizationTests")]
public class CollectionParallelizationTestsFixture
{

}

[Collection("ParallelizationTests")]
public class CollectionParallelizationTests1
{
    private readonly CollectionParallelTestFixture parallelTestFixture;

    public CollectionParallelizationTests1(CollectionParallelTestFixture parallelTestFixture)
    {
        this.parallelTestFixture = parallelTestFixture;
    }

    [Fact]
    public void TestIncrement1()
    {
        var expected = 2;
        var actual = parallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }


}

[Collection("ParallelizationTests")]
public class CollectionParallelizationTests2
{
    private readonly CollectionParallelTestFixture parallelTestFixture;

    public CollectionParallelizationTests2(CollectionParallelTestFixture parallelTestFixture)
    {
        this.parallelTestFixture = parallelTestFixture;
    }

    [Fact]
    //d
    public void TestIncrement1()
    {
        var expected = 2;
        var actual = parallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

}
