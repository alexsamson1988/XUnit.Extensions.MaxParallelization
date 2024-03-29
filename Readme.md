# XUnit.Extensions.MaxParallelization
This library is there to run every test cases in parallel by default. It also changes how Fixture are injected in XUnit.

## Setup
Install the nuget package:
`NuGet\Install-Package XUnit.Extensions.MaxParallelization`

Then you need to add the following assembly attribute: 
`[assembly: TestFramework("XUnit.Extensions.MaxParallelization.ParallelTestFramework", "XUnit.Extensions.MaxParallelization")]`

## Parallelization
With this library every test cases, classes and collections will run in parallel. Therefore if you don't want a test case, a class or a collection to run in parallel you will need to set the `DisableParallelization` attribute over the class or the test method on which you do not want to run in parallel.+6458
For collections you need to set it inside the `CollectionDefinition` attribute like so:
`[CollectionDefinition("DisableParallelization",DisableParallelization = true)]`

## Fixture Injection
This extension change the way dependency injection is handled in XUnit. I wanted to make this more like it is handled in ASP.Net.
```
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
```
The `FixtureRegisterationLevel` parameter is there to declare the scope of the dependency.

`Assembly` will create a singleton for the whole assembly

`Collection` create an instance that will be shared among all the tests cases in the same collections 

`Class` create an instance that will be shared among tests cases in the same class

`Method` Create an instance for each test case.

You can pass an instance as a parameter but unless the scope is assembly the dependency will be reinstanciated at some point.

This code is greatly inspired by [this](https://github.com/meziantou/Meziantou.Xunit.ParallelTestFramework) repository from [Gérald Barré](https://github.com/meziantou).