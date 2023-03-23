using AutoFixture;
using Mono.Reflection;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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
        return GetContainer();
    }

    private FixtureContainer GetContainer()
    {
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
            .Select(fixtureRegistration => new FixtureRegistration(fixtureRegistration.FixtureType,fixtureRegistration.FixtureInstanceType, fixtureRegistration.RegisterationLevel,fixtureRegistration.BuildAction))
            .GroupBy(fixtureRegistration => fixtureRegistration.FixtureType)
            .Select(group => group.First())
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
            fixture.Instance = BuildFixture(fixture);
        }
    }

    private IList<FixtureRegistration> GetFixturesToBuild()
    {
        return fixtureRegistrations
            .Where(fixtureRegistration => fixtureRegistration.RegisterationLevel == fixtureRegisterationLevel && (fixtureRegistration.Instance == null || fixtureRegisterationLevel != FixtureRegisterationLevel.Assembly))
            .OrderBy(registration => GetInterdependencyCount(registration.FixtureInstanceType))
            .ToList();
    }

    private int GetInterdependencyCount(Type type)
    {
        return type.GetConstructors()
                   .OrderBy(ctor => ctor.GetParameters().Length)
                   .First()
                   .GetParameters()
                   .Count(parameter => fixtureRegistrations.Any(registration => registration.FixtureType == parameter.ParameterType));
    }

    private object BuildFixture(FixtureRegistration fixtureRegistration)
    {
        var type = fixtureRegistration.FixtureInstanceType;
        
        if (fixtureRegistration.BuildAction != null)
        {
            var types = GetTypesNeededForBuildAction(fixtureRegistration.BuildAction);
            GetInstancesByTypes(types);
            return fixtureRegistration.BuildAction.Invoke(GetTemporaryContainer());
        }    
        else
        {
            var ctor = GetValidConstructor(type);
            return BuildFixtureUsingConstructor(ctor);
        }
        throw new Exception($"Container cannot instanciate type {type.Name} you need to either declare a parameterless constructor or provide all needed dependencies and sub dependencies needed to build the fixture on the container");
    }

    private ConstructorInfo GetValidConstructor(Type type)
    {
        var ctors = type.GetConstructors();
        foreach (var ctor in ctors.OrderBy(c => c.GetParameters().Length))
        {
            if (ctor.GetParameters().Length == 0)
                return ctor;
            if(AllParametersInFixtures(ctor))
                return ctor;
        }
        throw new Exception($"Container cannot instanciate type {type.Name} you need to either declare a parameterless constructor or provide all needed dependencies and sub dependencies needed to build the fixture on the container");
    }

    private IList<Type> GetTypesNeededForBuildAction(Func<FixtureContainer, object> func)
    {
        var output = new List<Type>();  
        MethodInfo getRequiredFixtureMethod = typeof(FixtureContainer).GetMethod("GetRequiredFixture");

        MethodInfo delegateTargetMethod = func.GetMethodInfo();
        
        var instructions = Disassembler.GetInstructions(delegateTargetMethod);

        var callInstructions = instructions.Where(instruction => instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt);

        foreach (var callInstruction in callInstructions)
        {
            MethodInfo calledMethod = callInstruction.Operand as MethodInfo;

            // Check if the called method is a generic instance of GetRequiredFixture<T>
            if (calledMethod != null && calledMethod.IsGenericMethod && calledMethod.GetGenericMethodDefinition() == getRequiredFixtureMethod)
            {
                Type[] typeArguments = calledMethod.GetGenericArguments();
                output.AddRange(typeArguments);
            }
        }

        return output;
    }

    private FixtureContainer GetTemporaryContainer()
    {
        var currentContainer = GetContainer();
        if (parentContainer == null)
            return currentContainer;
        return FixtureContainerMerger.Merge(parentContainer, currentContainer);
    }

    private object? BuildFixtureUsingConstructor(ConstructorInfo ctor)
    {
        

        if (ctor.GetParameters().Length == 0)
        {
            return ctor.Invoke(null);
        }

        var parametersTypes = ctor.GetParameters().Select(parameter => parameter.ParameterType).ToList();
        var constructorParameters = GetInstancesByTypes(parametersTypes);

        if (!AllParametersInFixtures(ctor))
            throw new Exception($"Container cannot instanciate type {ctor.Name} you need to either declare a parameterless constructor or provide all needed dependencies and sub dependencies needed to build the fixture on the container");

        return ctor.Invoke(constructorParameters);
    }

    private bool AllParametersInFixtures(ConstructorInfo ctor)
    {
        return ctor.GetParameters().All(p => IsTypeInFixtures(p.ParameterType));
    }

    private object[] GetInstancesByTypes(IList<Type> types)
    {
        return types.Select(GetInstanceByType).ToArray();
    }

    private object GetInstanceByType(Type type)
    {
        var fixtureRegistration = GetFixtureRegistration(type);
        if (fixtureRegistration == null)
            throw new Exception($"Container does not contain reference for the type {type.Name}");
        if (fixtureRegistration.Instance == null)
            fixtureRegistration.Instance = BuildFixture(fixtureRegistration);

        return fixtureRegistration.Instance;
    }

    private FixtureRegistration? GetFixtureRegistration(Type fixtureType)
    {
        var fixtureRegistration = fixtureRegistrations.FirstOrDefault(fixture => fixture.FixtureType == fixtureType);
        if(fixtureRegistration == null && parentContainer != null)
        {
            var fixture = parentContainer.Fixtures.FirstOrDefault(fixture => fixture.Key == fixtureType);
            fixtureRegistration = new FixtureRegistration(fixtureType, fixtureType, fixture.Value, parentContainer.GetContainerLevel(),null);
        }
        return fixtureRegistration;
    }

    private bool IsTypeInFixtures(Type type)
    {
        return fixtureRegistrations.Any(fixtureRegistration => fixtureRegistration.FixtureType == type) ||
               (parentContainer != null && parentContainer.Fixtures.Any(fixtureRegistration => fixtureRegistration.Key == type));
    }

}
