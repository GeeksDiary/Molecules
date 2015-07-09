using Molecules.Core.Depenencies;

namespace Molecules.Core.Runtime
{
    public class Configuration
    {
        public AtomProcessor Processor { get; private set; }

        public IDependencyResolver DependencyResolver { get; private set; }

        public void UseProcessor(AtomProcessor processor)
        {
            Processor = processor;
        }

        public void UseDependencyResolver(IDependencyResolver dependencyResolver)
        {
            DependencyResolver = dependencyResolver;
        }
    }
}