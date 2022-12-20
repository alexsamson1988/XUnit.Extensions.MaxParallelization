namespace XUnit.Extensions.MaxParallelization.Tests.OrderingTests;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class OrderAttribute: Attribute
{
	public int Order { get; set; }
	public OrderAttribute(int order)
	{
        Order = order;
    }
}
