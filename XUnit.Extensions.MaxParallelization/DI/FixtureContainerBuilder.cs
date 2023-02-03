using System.Reflection;

namespace XUnit.Extensions.MaxParallelization.DI;
public class FixtureContainerBuilder
{
    private IList<FixtureRegistration> fixtureRegistrations;
    private FixtureRegisterationLevel fixtureRegisterationLevel;
    private FixtureContainer? parentContainer;
    public FixtureContainer BuildContainer(
         FixtureRegistrationCollection fixtureRegistrationCollection,
        FixtureRegisterationLevel fixtureRegisterationLevel,
        FixtureContainer? parentContainer)
    {
        this.parentContainer = parentContainer;
        this.fixtureRegisterationLevel = fixtureRegisterationLevel;
        fixtureRegistrations = fixtureRegistrationCollection
                                                            .GetFixtureRegistrations()
                                                            .Where(fixtureRegistration => fixtureRegistration.RegisterationLevel == fixtureRegisterationLevel)
                                                            .Select(m => new FixtureRegistration(m.FixtureType,m.RegisterationLevel))
                                                            .ToList();
        if (!fixtureRegistrations.Any())
            return new FixtureContainer(new List<FixtureRegistration>(),fixtureRegisterationLevel);

        var needToBuildContainer = fixtureRegistrations.Any(fixture => fixture.Instance == null) || fixtureRegisterationLevel != FixtureRegisterationLevel.Assembly;
        if (!needToBuildContainer)
            return new FixtureContainer(fixtureRegistrations, fixtureRegisterationLevel);

        foreach (var fixture in GetFixturesToBuild())
        {
            fixture.Instance = BuildFixture(fixture.FixtureType);
        }
        return new FixtureContainer(fixtureRegistrations, fixtureRegisterationLevel);
    }

    private IList<FixtureRegistration> GetFixturesToBuild()
    {
        return fixtureRegistrations
            .Where(fixtureRegistration => fixtureRegistration.RegisterationLevel == fixtureRegisterationLevel && ( fixtureRegistration.Instance == null ||  fixtureRegisterationLevel != FixtureRegisterationLevel.Assembly))
            .ToList();
    }

    private object BuildFixture(Type type)
    {
        var ctors = type.GetConstructors();
        
        foreach (var ctor in ctors.OrderBy(c => c.GetParameters().Length))
        {
            var fixtureInstance = BuildFixtureUsingConstructor(ctor);
            if (fixtureInstance != null)
                return fixtureInstance;
        }
        

        throw new Exception($"Container cannot instanciate type {type.Name} you need to either declare a parameterless constructor or provide all needed dependencies and sub dependencies needed to build the fixture on the container");
    }

    private object? BuildFixtureUsingConstructor(ConstructorInfo ctor)
    {
        if (ctor.GetParameters().Length == 0)
        {
            return ctor.Invoke(null);
        }
        var parametersAreAllInFixtures = ctor.GetParameters().All(p => IsTypeInFixtures(p.ParameterType));

        if (!parametersAreAllInFixtures)
            return null;

        var constructorParameters = GetConstructorParameters(ctor);

        return ctor.Invoke(constructorParameters);
    }
    private object[] GetConstructorParameters(ConstructorInfo ctor)
    {
        List<object> ctorParams = new List<object>();
        foreach (var parameterInfos in ctor.GetParameters())
        {
            var parameterType = parameterInfos.ParameterType;
            var fixtureRegistration = GetFixtureRegistration(parameterType);
            if (fixtureRegistration == null)
                throw new Exception($"Container does not contain reference for the type {parameterType.Name}");
            if (fixtureRegistration.Instance == null)
                fixtureRegistration.Instance = BuildFixture(parameterInfos.GetType());

            ctorParams.Add(fixtureRegistration.Instance);
        }
        return ctorParams.ToArray();
    }

    private FixtureRegistration? GetFixtureRegistration(Type fixtureType)
    {
        var fixtureRegistration = fixtureRegistrations.FirstOrDefault(fixture => fixture.FixtureType == fixtureType);
        if(fixtureRegistration == null && parentContainer != null)
        {
            var fixture = parentContainer.Fixtures.FirstOrDefault(fixture => fixture.Key == fixtureType);
            fixtureRegistration = new FixtureRegistration(fixtureType, fixture.Value, parentContainer.ContainerLevel);
        }
        return fixtureRegistration;
    }

    private bool IsTypeInFixtures(Type type)
    {
        return fixtureRegistrations.Any(fixtureRegistration => fixtureRegistration.FixtureType == type) ||
               (parentContainer != null && parentContainer.Fixtures.Any(fixtureRegistration => fixtureRegistration.Key == type));
    }

}
