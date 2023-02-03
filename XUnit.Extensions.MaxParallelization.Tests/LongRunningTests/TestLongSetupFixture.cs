namespace XUnit.Extensions.MaxParallelization.Tests.LongRunningTests;
public class TestLongSetupFixture : IAsyncLifetime
{
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(10000);
    }
}
