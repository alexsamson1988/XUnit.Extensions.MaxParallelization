namespace XUnit.Extensions.MaxParallelization.DI;
/// <summary>
/// Represents a collection of fixture registrations for a testing framework.
/// </summary>
public class FixtureRegistrationCollection
{
    private IList<FixtureRegistration> fixtures = new List<FixtureRegistration>();

    /// <summary>
    /// Adds an assembly-level fixture registration.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <param name="instance">The optional instance of the implementation. Default is null.</param>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddAssemblyFixture<TInterface, TImplementation>(
        TImplementation? instance = default, 
        Func<FixtureContainer, TImplementation>? buildAction = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        return AddFixture<TInterface, TImplementation>(FixtureRegisterationLevel.Assembly, buildAction);
    }

    /// <summary>
    /// Adds an assembly-level fixture registration with the same interface and implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <param name="instance">The optional instance of the implementation. Default is null.</param>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddAssemblyFixture<TImplementation>(
        TImplementation? instance = default, 
        Func<FixtureContainer, TImplementation>? buildAction = null)
        where TImplementation : class
    {
        return AddFixture<TImplementation, TImplementation>(instance, FixtureRegisterationLevel.Assembly,buildAction);
    }

    /// <summary>
    /// Adds a collection-level fixture registration.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddCollectionFixture<TInterface, TImplementation>(Func<FixtureContainer, TImplementation>? buildAction = null)
         where TInterface : class
         where TImplementation : class, TInterface
    {
        return AddFixture<TInterface, TImplementation>(FixtureRegisterationLevel.Collection,buildAction);
    }

    /// <summary>
    /// Adds a collection-level fixture registration with the same interface and implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddCollectionFixture<TImplementation>(Func<FixtureContainer, TImplementation>? buildAction = null)
        where TImplementation: class
    {
        return AddFixture<TImplementation, TImplementation>(FixtureRegisterationLevel.Collection,buildAction);
    }

    /// <summary>
    /// Adds a class-level fixture registration.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddClassFixture<TInterface, TImplementation>(Func<FixtureContainer, TImplementation>? buildAction = null)
         where TInterface : class
         where TImplementation : class, TInterface
    {
        return AddFixture<TInterface, TImplementation>(FixtureRegisterationLevel.Class, buildAction);
    }

    /// <summary>
    /// Adds a class-level fixture registration with the same interface and implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddClassFixture<TImplementation>(Func<FixtureContainer, TImplementation>? buildAction = null)
        where TImplementation : class
    {
        return AddFixture<TImplementation, TImplementation>(FixtureRegisterationLevel.Class, buildAction);
    }

    /// <summary>
    /// Adds a test method-level fixture registration.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddTestMethodFixture<TInterface, TImplementation>(Func<FixtureContainer, TImplementation>? buildAction = null)
         where TInterface : class
         where TImplementation : class, TInterface
    {
        return AddFixture<TInterface, TImplementation>(FixtureRegisterationLevel.Method,buildAction);
    }

    /// <summary>
    /// Adds a test method-level fixture registration with the same interface and implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddTestMethodFixture<TImplementation>(Func<FixtureContainer, TImplementation>? buildAction = null)
        where TImplementation : class
    {
        return AddFixture<TImplementation>(FixtureRegisterationLevel.Method, buildAction);
    }

    /// <summary>
    /// Adds a fixture registration with the specified registration level and optional instance.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <param name="instance">The optional instance of the implementation. Default is null.</param>
    /// <param name="registerationLevel">The level at which the fixture should be registered.</param>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddFixture<TInterface, TImplementation>(
        TImplementation? instance, 
        FixtureRegisterationLevel registerationLevel = FixtureRegisterationLevel.Assembly,
        Func<FixtureContainer, TImplementation>? buildAction = null)
         where TInterface : class
         where TImplementation : class, TInterface
    {
        if (registerationLevel != FixtureRegisterationLevel.Assembly && instance != null)
            instance = default(TImplementation);
        var fixtureRegistration = new FixtureRegistration(
            typeof(TInterface), 
            typeof(TImplementation), 
            instance, 
            registerationLevel, 
            buildAction);
        fixtures.Add(fixtureRegistration);
        return this;
    }

    /// <summary>
    /// Adds a fixture registration with the same interface and implementation type, and optional instance.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <param name="instance">The optional instance of the implementation. Default is null.</param>
    /// <param name="registerationLevel">The level at which the fixture should be registered. Default is Assembly.</param>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddFixture<TImplementation>(
        TImplementation? instance, 
        FixtureRegisterationLevel registerationLevel = FixtureRegisterationLevel.Assembly,
        Func<FixtureContainer, TImplementation>? buildAction = null)
        where TImplementation : class
    {
        if (registerationLevel != FixtureRegisterationLevel.Assembly && instance != null)
            instance = default(TImplementation);
        var fixtureRegistration = new FixtureRegistration(typeof(TImplementation), typeof(TImplementation), instance, registerationLevel, buildAction);
        fixtures.Add(fixtureRegistration);
        return this;
    }

    /// <summary>
    /// Adds a fixture registration with the specified interface type and default implementation type.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <param name="registerationLevel">The level at which the fixture should be registered. Default is Assembly.</param>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddFixture<TInterface, TImplementation>(
        FixtureRegisterationLevel registerationLevel = FixtureRegisterationLevel.Assembly,
        Func<FixtureContainer, TImplementation>? buildAction = null)
         where TInterface : class
         where TImplementation : class, TInterface
    {
        return AddFixture<TInterface, TImplementation>(default, registerationLevel);
    }

    /// <summary>
    /// Adds a fixture registration with the default interface and implementation types.
    /// </summary>
    /// <param name="registerationLevel">The level at which the fixture should be registered. Default is Assembly.</param>
    /// <returns>The updated FixtureRegistrationCollection.</returns>
    public FixtureRegistrationCollection AddFixture<TImplementation>(
        FixtureRegisterationLevel registerationLevel = FixtureRegisterationLevel.Assembly,
        Func<FixtureContainer, TImplementation>? buildAction = null)
         where TImplementation : class
    {
        return AddFixture<TImplementation, TImplementation>(default, registerationLevel, buildAction);
    }

    public IList<FixtureRegistration> GetFixtureRegistrations()
    {
        return fixtures;
    }

}
