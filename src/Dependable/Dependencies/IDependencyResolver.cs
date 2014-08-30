using System;

namespace Dependable.Dependencies
{
    public interface IDependencyResolver : IDisposable
    {
        IDependencyScope BeginScope();
    }
}