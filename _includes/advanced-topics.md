## <a name="dependency-resolvers" class="anchor"></a>Dependency Resolvers
Real world job implementations can be quite significant and you may want to use composition techniques you are familiar with when building them. Dependable has extension two extension points to hook your IoC container to use all the composition goodness. 

```csharp
public interface IDependencyResolver : IDisposable
{
    IDependencyScope BeginScope();
}

public interface IDependencyScope : IDisposable
{
    object GetService(Type type);

    IEnumerable<object> GetServices(Type type);

    bool TryGetService(Type type, out object service);
}
```