using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnit.Extensions.MaxParallelization.DI;
public class FixtureRegistration
{
    public FixtureRegistration(Type fixtureType, object instance, FixtureRegisterationLevel registerationLevel)
    {
        FixtureType = fixtureType;
        Instance = instance;
        RegisterationLevel = registerationLevel;
    }

    public FixtureRegistration(Type fixtureType,  FixtureRegisterationLevel registerationLevel)
    {
        FixtureType = fixtureType;
        RegisterationLevel = registerationLevel;
    }

    public Type FixtureType { get; set; }
    public object Instance { get; set; }
    public FixtureRegisterationLevel RegisterationLevel { get; set; }
}
