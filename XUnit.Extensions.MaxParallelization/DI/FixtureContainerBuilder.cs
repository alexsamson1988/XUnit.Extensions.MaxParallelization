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
        InitializeProperties(fixtureRegisterationLevel, parentContainer);
        PopulateFixtureRegistrations(fixtureRegistrationCollection);

        if (!fixtureRegistrations.Any())
            return CreateEmptyFixtureContainer();

        if (!NeedToBuildContainer())
            return new FixtureContainer(fixtureRegistrations, fixtureRegisterationLevel);

        BuildFixtures();
        return new FixtureContainer(fixtureRegistrations, fixtureRegisterationLevel);
    }

    private void InitializeProperties(FixtureRegisterationLevel fixtureRegisterationLevel, FixtureContainer? parentContainer)
    {
        this.parentContainer = parentContainer;
        this.fixtureRegisterationLevel = fixtureRegisterationLevel;
    }

    private void PopulateFixtureRegistrations(FixtureRegistrationCollection fixtureRegistrationCollection)
    {
        fixtureRegistrations = fixtureRegistrationCollection
            .GetFixtureRegistrations()
            .Where(fixtureRegistration => fixtureRegistration.RegisterationLevel == fixtureRegisterationLevel)
            .Select(fixtureRegistration => new FixtureRegistration(fixtureRegistration.FixtureType,fixtureRegistration.FixtureInstanceType, fixtureRegistration.RegisterationLevel))
            .ToList();
    }

    private FixtureContainer CreateEmptyFixtureContainer()
    {
        return new FixtureContainer(new List<FixtureRegistration>(), fixtureRegisterationLevel);
    }

    private bool NeedToBuildContainer()
    {
        return fixtureRegistrations.Any(fixture => fixture.Instance == null) || fixtureRegisterationLevel != FixtureRegisterationLevel.Assembly;
    }

    private void BuildFixtures()
    {
        foreach (var fixture in GetFixturesToBuild())
        {
            fixture.Instance = BuildFixture(fixture.FixtureInstanceType);
        }
    }

    private IList<FixtureRegistration> GetFixturesToBuild()
    {
        return fixtureRegistrations
            .Where(fixtureRegistration => fixtureRegistration.RegisterationLevel == fixtureRegisterationLevel && (fixtureRegistration.Instance == null || fixtureRegisterationLevel != FixtureRegisterationLevel.Assembly))
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

        if (!AllParametersInFixtures(ctor))
            return null;

        var constructorParameters = GetConstructorParameters(ctor);

        return ctor.Invoke(constructorParameters);
    }

    private bool AllParametersInFixtures(ConstructorInfo ctor)
    {
        return ctor.GetParameters().All(p => IsTypeInFixtures(p.ParameterType));
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
            fixtureRegistration = new FixtureRegistration(fixtureType, fixtureType, fixture.Value, parentContainer.ContainerLevel);
        }
        return fixtureRegistration;
    }

    private bool IsTypeInFixtures(Type type)
    {
        return fixtureRegistrations.Any(fixtureRegistration => fixtureRegistration.FixtureType == type) ||
               (parentContainer != null && parentContainer.Fixtures.Any(fixtureRegistration => fixtureRegistration.Key == type));
    }

}
