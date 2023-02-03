namespace XUnit.Extensions.MaxParallelization.Tests.LongRunningTests;
public class DependentFixture
{
	public DependentFixture(TestLongSetupFixture testLongSetupFixture)
	{
		
	}

	public async Task FakeAct(int numberOfSeconds)
	{
		await Task.Delay(numberOfSeconds * 1000);
	}
}
