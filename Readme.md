#XUnit.Extensions.MaxParallelization

##Setup
You need to add the following assembly attribute: 
`[assembly: TestFramework("XUnit.Extensions.MaxParallelization.ParallelTestFramework", "XUnit.Extensions.MaxParallelization")]`

##Parallelization
With this library every test cases, classes and collections will run in parallel. Therefore if you don't want a test case a class or a collection you will need to set the `DisableParallelization` attribute over the class or the test method on which you do not want to run in parallel.

For collections you need to set it inside the `CollectionDefinition` attribute likeso:
`[CollectionDefinition("DisableParallelization",DisableParallelization = true)]`

##Dependency Injection
This extension change the way dependency injection is handled in XUnit. I wanted to make this more like it is handled in ASP.Net.
```
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
```
The `FixtureRegisterationLevel` is there to declare the scope of the dependency.
`Assembly` will create a singleton for the whole assembly
`Collection` create an instance that will be shared among all the tests cases in the same collections 
`Class` create an instance that will be shared among tests cases in the same class
`Method` Create an instance for each test case.

You can pass an instance as a parameter but unless the scope is assembly the dependency will be reinstanciated at some point.

