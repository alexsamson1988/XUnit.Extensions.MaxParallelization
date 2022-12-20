using XUnit.Extensions.MaxParallelization.Tests.Fixtures;

namespace XUnit.Extensions.MaxParallelization.Tests.OrderingTests;
[TestCaseOrderer("XUnit.Extensions.MaxParallelization.Tests.TestOrderer", "XUnit.Extensions.MaxParallelization.Tests")]
public class OrderingTests
{
    private readonly OrderingFixture orderingFixture;

    public OrderingTests(OrderingFixture orderingFixture)
    {
        this.orderingFixture = orderingFixture;
    }

    [Fact, Order(1)]
    public void TestAdd1()
    {
        orderingFixture.Add(1);
        Assert.Equal(1, orderingFixture.Count);
    }

    [Fact, Order(2)]
    public void TestAdd2()
    {
        orderingFixture.Add(1);
        Assert.Equal(2, orderingFixture.Count);
    }

    [Fact, Order(3)]
    public void TestAdd3()
    {
        orderingFixture.Add(1);
        Assert.Equal(3, orderingFixture.Count);
    }

    [Fact, Order(4)]
    public void TestAdd4()
    {
        orderingFixture.Add(1);
        Assert.Equal(4, orderingFixture.Count);
    }

}
