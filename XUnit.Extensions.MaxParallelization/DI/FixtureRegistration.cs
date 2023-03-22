using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnit.Extensions.MaxParallelization.DI;
public class FixtureRegistration
{
    public FixtureRegistration(Type fixtureType,Type fixtureInstanceType, object instance, FixtureRegisterationLevel registerationLevel)
    {
        FixtureType = fixtureType;
        FixtureInstanceType = fixtureInstanceType;
        Instance = instance;
        RegisterationLevel = registerationLevel;
    }

    public FixtureRegistration(Type fixtureType, Type fixtureInstanceType, FixtureRegisterationLevel registerationLevel)
    {
        FixtureType = fixtureType;
        FixtureInstanceType = fixtureInstanceType;
        RegisterationLevel = registerationLevel;
    }

    public Type FixtureType { get; set; }
    public Type FixtureInstanceType { get; set; }
    public object Instance { get; set; }
    public FixtureRegisterationLevel RegisterationLevel { get; set; }
}
