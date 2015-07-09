using System;

namespace Molecules.Core.Depenencies
{
    public class DefaultDependencyResolver : IDependencyResolver
    {
        static readonly IDependencyScope Default = new DefaultDependecyScope();

        public IDependencyScope BeginScope()
        {
            return Default;
        }
    }

    public class DefaultDependecyScope : IDependencyScope
    {
        public void Dispose()
        {
        }

        public T Resolve<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}