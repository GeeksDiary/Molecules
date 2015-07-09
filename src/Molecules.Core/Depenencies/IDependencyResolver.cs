using System;

namespace Molecules.Core.Depenencies
{
    public interface IDependencyResolver
    {
        IDependencyScope BeginScope();
    }

    public interface IDependencyScope : IDisposable
    {
        T Resolve<T>();
    }
}