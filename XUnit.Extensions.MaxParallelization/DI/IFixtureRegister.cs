using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnit.Extensions.MaxParallelization.DI;
public interface IFixtureRegister
{
    void RegisterFixtures(FixtureRegistrationCollection container);
}
