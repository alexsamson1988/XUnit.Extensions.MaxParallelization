namespace XUnit.Extensions.MaxParallelization.Tests.LongRunningTests;
public class NestedTests
{
    private readonly DependentFixture dependentFixture;

    public NestedTests(DependentFixture dependentFixture)
    {
        this.dependentFixture = dependentFixture;
    }

    [Fact]
    public async Task Test1()
    {
        await this.dependentFixture.FakeAct(1);
    }

    [Fact]
    public async Task Test2()
    {
        await this.dependentFixture.FakeAct(2);
    }

    [Fact]
    public async Task Test3()
    {
        await this.dependentFixture.FakeAct(3);
    }

    [Fact]
    public async Task Test4()
    {
        await this.dependentFixture.FakeAct(4);
    }

    [Fact]
    public async Task Test5()
    {
        await this.dependentFixture.FakeAct(4);
    }

    [Fact]
    public async Task Test6()
    {
        await this.dependentFixture.FakeAct(4);
    }

    [Fact]
    public async Task Test7()
    {
        await this.dependentFixture.FakeAct(4);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(3)]
    [InlineData(2)]
    public async Task Test8(int seconds)
    {
        await this.dependentFixture.FakeAct(seconds);
    }
}
