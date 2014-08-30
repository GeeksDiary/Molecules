using System;
using System.Collections.Generic;

namespace Dependable.Dependencies
{
    public interface IDependencyScope : IDisposable
    {
        object GetService(Type type);

        IEnumerable<object> GetServices(Type type);

        bool TryGetService(Type type, out object service);
    }
}