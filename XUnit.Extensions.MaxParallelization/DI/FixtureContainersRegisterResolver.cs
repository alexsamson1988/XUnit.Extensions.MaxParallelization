using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace XUnit.Extensions.MaxParallelization.DI;
public static class FixtureContainersRegisterResolver
{

    public static FixtureRegistrationCollection ResolveFixtureRegistrations(this ITestAssembly testAssembly)
    {
        var interfaceType = typeof(IFixtureRegister);
        var registers = testAssembly.Assembly.GetTypes(true).Where(s => s.Interfaces.Any(interfaceTypeInfo => interfaceTypeInfo.ToRuntimeType() == interfaceType));
        var container = new FixtureRegistrationCollection();
        foreach (var register in registers)
        {
            var registerInstance = (IFixtureRegister)register.ToRuntimeType().GetConstructors()[0].Invoke(null);
            registerInstance.RegisterFixtures(container);
        }

        return container;
    }
}
