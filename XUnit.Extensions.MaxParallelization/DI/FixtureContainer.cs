using Xunit;

namespace XUnit.Extensions.MaxParallelization.DI;
public class FixtureContainer: IAsyncLifetime
{
	public readonly FixtureRegisterationLevel ContainerLevel;
	private readonly Dictionary<Type, object> fixtures;
    public Dictionary<Type,object> Fixtures 
	{
        get
        { 
			return fixtures;
		} 
	}

	public FixtureContainer(IList<FixtureRegistration> fixtureRegistrations,FixtureRegisterationLevel containerLevel)
	{
		ContainerLevel = containerLevel;
        fixtures = new Dictionary<Type, object>();
		RegisterFixtureInContainer(fixtureRegistrations);
    }

    public FixtureContainer(Dictionary<Type,Object> fixtures, FixtureRegisterationLevel containerLevel)
    {
        ContainerLevel = containerLevel;
        this.fixtures = fixtures;
    }

    private void RegisterFixtureInContainer(IList<FixtureRegistration> fixtureRegistrations)
	{
		foreach (var fixtureRegistration in fixtureRegistrations)
			fixtures.Add(fixtureRegistration.FixtureType, fixtureRegistration.Instance);
	}

    public Task InitializeAsync()
    {
		var fixturesToInitialize = fixtures.Where(fixture => fixture.Key.GetInterface(nameof(IAsyncLifetime)) != null)
																		.Select(fixture => ((IAsyncLifetime)fixture.Value).InitializeAsync());
		return Task.WhenAll(fixturesToInitialize);
    }

    public Task DisposeAsync()
	{
		var fixtureToDisposes = fixtures.Where(fixture => fixture.Key.GetInterface(nameof(IDisposable)) != null)
                                                          .Select(fixture => new Task(() => ((IDisposable)fixture.Value).Dispose()))
										.Union(fixtures.Where(fixture => fixture.Key.GetInterface(nameof(IAsyncLifetime)) != null)
                                                          .Select(fixture => ((IAsyncLifetime)fixture.Value).DisposeAsync()));
        return Task.WhenAll(fixtureToDisposes);
    }
    
}
