using System;
using System.Collections.Generic;

namespace Dependable.Dependencies
{
    public class DefaultDependencyScope : IDependencyScope
    {
        public void Dispose()
        {
        }

        public object GetService(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public IEnumerable<object> GetServices(Type type)
        {
            return new[] { GetService(type) };
        }

        public bool TryGetService(Type type, out object service)
        {
            service = null;
            return false;
        }
    }
}