using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;
using XUnit.Extensions.MaxParallelization.DI;

namespace XUnit.Extensions.MaxParallelization;
public static class ConstructorArgumentBuilder
{
    private static ConstructorInfo SelectTestClassConstructor(IReflectionTypeInfo classInfos,ExceptionAggregator aggregator)
    {
        var result = classInfos.Type.GetConstructors().FirstOrDefault(ci => !ci.IsStatic);

        return result;
    }

    public static object[] CreateTestClassConstructorArguments(this IReflectionTypeInfo classInfos,FixtureContainer fixtureContainer, ExceptionAggregator aggregator)
    {
        var isStaticClass = classInfos.Type.GetTypeInfo().IsAbstract && classInfos.Type.GetTypeInfo().IsSealed;
        if (!isStaticClass)
        {
            var ctor = SelectTestClassConstructor(classInfos,aggregator);
            if (ctor != null)
            {
                var unusedArguments = new List<Tuple<int, ParameterInfo>>();
                var parameters = ctor.GetParameters();

                object[] constructorArguments = new object[parameters.Length];
                for (int idx = 0; idx < parameters.Length; ++idx)
                {
                    var parameter = parameters[idx];
                    object? argumentValue = TryGetConstructorArgument(parameter, fixtureContainer);

                    if (argumentValue is not null)
                        constructorArguments[idx] = argumentValue;
                    else if (parameter.HasDefaultValue)
                        constructorArguments[idx] = parameter.DefaultValue;
                    else if (parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
                        constructorArguments[idx] = Array.CreateInstance(parameter.ParameterType, 0);
                    else
                        unusedArguments.Add(Tuple.Create(idx, parameter));
                }

                if (unusedArguments.Count > 0)
                    aggregator.Add(new TestClassException(FormatConstructorArgsMissingMessage(ctor, unusedArguments)));

                return constructorArguments;
            }
        }

        return new object[0];
    }

    private static object? TryGetConstructorArgument(ParameterInfo parameterInfo, FixtureContainer fixtureContainer)
    {
        var paramType = parameterInfo.ParameterType;
        var canGetValue = fixtureContainer.Fixtures.TryGetValue(paramType, out var value);
        if (!canGetValue)
            return null;
        return value;
    }

    private static string FormatConstructorArgsMissingMessage(ConstructorInfo constructor, IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments)
            => $"The following constructor parameters did not have matching arguments: {string.Join(", ", unusedArguments.Select(arg => $"{arg.Item2.ParameterType.Name} {arg.Item2.Name}"))}";
}

