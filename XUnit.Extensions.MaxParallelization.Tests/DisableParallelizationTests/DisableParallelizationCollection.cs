using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.DisableParallelizationTests;

[CollectionDefinition("DisableParallelization",DisableParallelization = true)]
public class DisableParallelizationCollectionDefinition
{

}

[Collection("DisableParallelization")]
public class DisableParallelizationCollection1
{
    private readonly CollectionParallelTestFixture collectionParallelTestFixture;

    public DisableParallelizationCollection1(CollectionParallelTestFixture collectionParallelTestFixture)
	{
        this.collectionParallelTestFixture = collectionParallelTestFixture;
    }
    [Fact]
    public void TestParallel1()
    {
        var expected = collectionParallelTestFixture.GetCount() + 1;
        var actual = collectionParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestParallel2()
    {
        var expected = collectionParallelTestFixture.GetCount() + 1;
        var actual = collectionParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestParallel3()
    {
        var expected = collectionParallelTestFixture.GetCount() + 1;
        var actual = collectionParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }
}

[Collection("DisableParallelization")]
public class DisableParallelizationCollection2
{
    private readonly CollectionParallelTestFixture collectionParallelTestFixture;

    public DisableParallelizationCollection2(CollectionParallelTestFixture collectionParallelTestFixture)
    {
        this.collectionParallelTestFixture = collectionParallelTestFixture;
    }
    [Fact]
    public void TestParallel4()
    {
        var expected = collectionParallelTestFixture.GetCount() + 1;
        var actual = collectionParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestParallel5()
    {
        var expected = collectionParallelTestFixture.GetCount() + 1;
        var actual = collectionParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestParallel6()
    {
        var expected = collectionParallelTestFixture.GetCount() + 1;
        var actual = collectionParallelTestFixture.Call();
        Assert.Equal(expected, actual);
    }
}
