using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XUnit.Extensions.MaxParallelization.DI;
public class FixtureRegistrationCollection
{
    private IList<FixtureRegistration> fixtures = new List<FixtureRegistration>();

    public FixtureRegistrationCollection AddFixture<T>(T instance, FixtureRegisterationLevel registerationLevel = FixtureRegisterationLevel.Assembly)
    {
        var fixtureRegistration = new FixtureRegistration(typeof(T),instance, registerationLevel);
        fixtures.Add(fixtureRegistration);
        return this;
    }

    public FixtureRegistrationCollection AddFixture<T>(FixtureRegisterationLevel registerationLevel = FixtureRegisterationLevel.Assembly)
    {
        var fixtureRegistration = new FixtureRegistration(typeof(T), registerationLevel);
        fixtures.Add(fixtureRegistration);
        return this;
    }

    public IList<FixtureRegistration> GetFixtureRegistrations()
    {
        return fixtures;
    }

}
