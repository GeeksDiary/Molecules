using System;
using Autofac;

namespace Dependable.Extensions.Dependencies.Autofac
{
    public static class ConfigurationExtension
    {
        public static DependableConfiguration UseAutofacDependencyResolver(
            this DependableConfiguration configuration, 
            ILifetimeScope container)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (container == null) throw new ArgumentNullException("container");

            return configuration.UseDependencyResolver(new AutofacDependencyResolver(container));            
        }
    }
}