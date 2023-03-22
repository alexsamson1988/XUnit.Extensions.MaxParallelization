using XUnit.Extensions.MaxParallelization.DI;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.InterfaceTestFixture;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;
using XUnit.Extensions.MaxParallelization.Tests.LongRunningTests;

namespace XUnit.Extensions.MaxParallelization.Tests.Fixtures;
public class FixtureRegister : IFixtureRegister
{
    public void RegisterFixtures(FixtureRegistrationCollection container)
    {
        container.AddAssemblyFixture<TestLongSetupFixture>()
                 .AddAssemblyFixture<AssemblyParallelTestFixture>()
                 .AddCollectionFixture<CollectionParallelTestFixture>()
                 .AddClassFixture<ClassParallelTestFixture>()
                 .AddClassFixture<ISomeService, SomeService>()
                 .AddTestMethodFixture<MethodParallelTestFixture>()
                 .AddTestMethodFixture<DependentFixture>()
                 .AddClassFixture<OrderingFixture>();
                 
                 
    }
}
