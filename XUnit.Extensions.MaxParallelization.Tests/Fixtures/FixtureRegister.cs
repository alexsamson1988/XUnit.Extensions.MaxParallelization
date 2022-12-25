﻿using XUnit.Extensions.MaxParallelization.DI;
using XUnit.Extensions.MaxParallelization.Tests.Fixtures.Parallel;

namespace XUnit.Extensions.MaxParallelization.Tests.Fixtures;
public class FixtureRegister : IFixtureRegister
{
    public void RegisterFixtures(FixtureRegistrationCollection container)
    {
        container.AddFixture<OrderingFixture>()
                 .AddFixture<CollectionParallelTestFixture>(FixtureRegisterationLevel.Collection)
                 .AddFixture<AssemblyParallelTestFixture>(FixtureRegisterationLevel.Assembly)
                 .AddFixture<ClassParallelTestFixture>(FixtureRegisterationLevel.Class)
                 .AddFixture<MethodParallelTestFixture>(FixtureRegisterationLevel.Method);
    }
}