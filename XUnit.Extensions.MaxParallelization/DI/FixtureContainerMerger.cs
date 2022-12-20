using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnit.Extensions.MaxParallelization.DI;
public static class FixtureContainerMerger
{
    public static FixtureContainer Merge(FixtureContainer container1, FixtureContainer container2)
    {
        var mainContainer = container1.ContainerLevel > container2.ContainerLevel? container2 : container1;
        var secondContainer = container1.ContainerLevel > container2.ContainerLevel ? container1 : container2;
        var mergedFixtures = new Dictionary<Type, object>();
        foreach (var fixture in mainContainer.Fixtures)
        {
            mergedFixtures[fixture.Key] = fixture.Value;
        }

        foreach (var fixture in secondContainer.Fixtures)
        {
            mergedFixtures[fixture.Key] = fixture.Value;
        }

        return new FixtureContainer(mergedFixtures, secondContainer.ContainerLevel);
    }
}
