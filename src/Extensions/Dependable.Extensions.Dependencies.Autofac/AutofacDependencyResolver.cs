using System;
using Autofac;
using Dependable.Dependencies;

namespace Dependable.Extensions.Dependencies.Autofac
{
    public class AutofacDependencyResolver : IDependencyResolver
    {
        readonly ILifetimeScope _lifetimeScope;

        public AutofacDependencyResolver(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");
            _lifetimeScope = lifetimeScope;
        }

        public IDependencyScope BeginScope()
        {
            return new AutofacDependencyScope(_lifetimeScope.BeginLifetimeScope());
        }

        public void Dispose()
        {
            _lifetimeScope.Dispose();
        }
    }
}