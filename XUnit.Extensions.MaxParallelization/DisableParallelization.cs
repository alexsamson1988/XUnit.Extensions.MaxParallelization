using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace XUnit.Extensions.MaxParallelization;
public static  class DisableParallelization
{
    public static bool IsParallelizationDisabled(this ITestMethod testMethod)
    {
        return testMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any() ||
               testMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a => a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration))) ||
               testMethod.TestClass.IsParallelizationDisabled();
    }

    public static bool IsParallelizationDisabled(this ITestClass testClass)
    {
        return testClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any() ||
               testClass.Class.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).Any() ||
               testClass.TestCollection.IsParallelizationDisabled();
    }

    public static bool IsParallelizationDisabled(this ITestCollection testCollection)
    {
        var disabledParallelization = false;
        if(testCollection.CollectionDefinition != null)
        {
            var collectionDefinitionAttribute = testCollection.CollectionDefinition.ToRuntimeType().GetCustomAttributes<CollectionDefinitionAttribute>().FirstOrDefault();
            if(collectionDefinitionAttribute != null)
                disabledParallelization = collectionDefinitionAttribute.DisableParallelization;
            if(!disabledParallelization && testCollection.CollectionDefinition.GetCustomAttributes(typeof(DisableParallelization)).Any())
                disabledParallelization= true;
        }

        return disabledParallelization ||
               testCollection.TestAssembly.IsParallelizationDisabled();
    }

    public static bool IsParallelizationDisabled(this ITestAssembly testAssembly)
    {
        return testAssembly.Assembly.GetCustomAttributes(typeof(DisableParallelization)).Any();
    }

}
