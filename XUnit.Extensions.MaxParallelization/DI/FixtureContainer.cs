using Xunit;

namespace XUnit.Extensions.MaxParallelization.DI;
public class FixtureContainer: IAsyncLifetime
{
	private readonly FixtureRegisterationLevel _containerLevel;
	private readonly Dictionary<Type, object> _fixtures;
    public Dictionary<Type,object> Fixtures 
	{
        get
        { 
			return _fixtures;
		} 
	}

	public FixtureContainer(IList<FixtureRegistration> fixtureRegistrations,FixtureRegisterationLevel containerLevel)
	{
		_containerLevel = containerLevel;
        _fixtures = new Dictionary<Type, object>();
		RegisterFixtureInContainer(fixtureRegistrations);
    }

    public FixtureContainer(Dictionary<Type,Object> fixtures, FixtureRegisterationLevel containerLevel)
    {
        _containerLevel = containerLevel;
        this._fixtures = fixtures;
    }

    public TFixtureType? GetFixture<TFixtureType>()
    {
        var fixtureType = typeof(TFixtureType);
        if (_fixtures.ContainsKey(fixtureType))
            return default;
        return (TFixtureType)_fixtures[fixtureType];
    }

    public TFixtureType GetRequiredFixture<TFixtureType>()
    {
        var fixtureType = typeof(TFixtureType);
        if (!_fixtures.ContainsKey(fixtureType))
            throw new ArgumentException("fixture not in fixture container");
        return (TFixtureType)_fixtures[fixtureType];
    }

    public FixtureRegisterationLevel GetContainerLevel()
    {
        return _containerLevel;
    }

    private void RegisterFixtureInContainer(IList<FixtureRegistration> fixtureRegistrations)
	{
		foreach (var fixtureRegistration in fixtureRegistrations)
        {
            _fixtures.Add(fixtureRegistration.FixtureType, fixtureRegistration.Instance);
        }
	}

    public async Task InitializeAsync()
    {
		var fixturesToInitialize = _fixtures.Where(fixture => fixture.Key.GetInterface(nameof(IAsyncLifetime)) != null)
																		.Select(fixture => ((IAsyncLifetime)fixture.Value).InitializeAsync());
        await Task.WhenAll(fixturesToInitialize).ConfigureAwait(false);
    }

    public async Task DisposeAsync()
	{
		var fixtureToDisposes = _fixtures.Where(fixture => fixture.Key.GetInterface(nameof(IDisposable)) != null)
                                                          .Select(fixture => new Task(() => ((IDisposable)fixture.Value).Dispose()))
										.Union(_fixtures.Where(fixture => fixture.Key.GetInterface(nameof(IAsyncLifetime)) != null)
                                                          .Select(fixture => ((IAsyncLifetime)fixture.Value).DisposeAsync()));
        await Task.WhenAll(fixtureToDisposes).ConfigureAwait(false);
    }
    
}
