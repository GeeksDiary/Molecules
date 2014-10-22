using System;
using System.Collections.Generic;
using Autofac;
using Dependable.Dependencies;

namespace Dependable.Extensions.Dependencies.Autofac
{
    public class AutofacDependencyScope : IDependencyScope
    {
        readonly ILifetimeScope _lifetimeScope;

        public AutofacDependencyScope(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");
            _lifetimeScope = lifetimeScope;
        }

        public object GetService(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return _lifetimeScope.Resolve(type);
        }

        public IEnumerable<object> GetServices(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            var s = _lifetimeScope.Resolve(type);
            return s as IEnumerable<object> ?? new[] { s };
        }

        public bool TryGetService(Type type, out object service)
        {
            if (type == null) throw new ArgumentNullException("type");

            return _lifetimeScope.TryResolve(type, out service);
        }

        public void Dispose()
        {
            _lifetimeScope.Dispose();            
        }
    }
}