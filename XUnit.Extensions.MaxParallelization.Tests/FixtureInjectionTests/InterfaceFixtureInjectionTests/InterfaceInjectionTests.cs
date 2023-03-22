using XUnit.Extensions.MaxParallelization.Tests.Fixtures.InterfaceTestFixture;

namespace XUnit.Extensions.MaxParallelization.Tests.FixtureInjectionTests.InterfaceFixtureInjectionTests;
public class InterfaceInjectionTests
{
    private readonly ISomeService _someService;

    public InterfaceInjectionTests(ISomeService someService)
	{
        this._someService = someService;
    }

    [Fact]
    public void TestInterfaceInjection()
    {
        Assert.Equal("test susccessful", _someService.GetStringValue());
    }
}
