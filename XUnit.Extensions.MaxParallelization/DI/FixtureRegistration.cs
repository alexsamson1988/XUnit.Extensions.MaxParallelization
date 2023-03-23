namespace XUnit.Extensions.MaxParallelization.DI;
public class FixtureRegistration
{
    public FixtureRegistration(Type fixtureType, Type fixtureInstanceType, object instance, FixtureRegisterationLevel registerationLevel, Func<FixtureContainer, object>? buildAction = null)
    {
        FixtureType = fixtureType;
        FixtureInstanceType = fixtureInstanceType;
        Instance = instance;
        RegisterationLevel = registerationLevel;
        BuildAction = buildAction;
    }

    public FixtureRegistration(Type fixtureType, Type fixtureInstanceType, FixtureRegisterationLevel registerationLevel, Func<FixtureContainer, object>? buildAction = null)
    {
        FixtureType = fixtureType;
        FixtureInstanceType = fixtureInstanceType;
        RegisterationLevel = registerationLevel;
        BuildAction = buildAction;
    }

    public Type FixtureType { get; set; }
    public Type FixtureInstanceType { get; set; }
    public object Instance { get; set; }
    public Func<FixtureContainer, object>? BuildAction { get; set; }
    public FixtureRegisterationLevel RegisterationLevel { get; set; }
}
