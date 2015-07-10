using Molecules.Core.Depenencies;

namespace Molecules.Core
{
    public class AtomContext
    {
        readonly IDependencyScope _scope;

        public AtomContext(IDependencyScope scope)
        {
            _scope = scope;
        }
        
        public T Resolve<T>()
        {
            return _scope.Resolve<T>();
        }

        public virtual AtomContext Clone()
        {
            return new AtomContext(_scope);
        }

        public AtomContext<T> Clone<T>(T input)
        {
            return new AtomContext<T>(_scope, input);
        }
    }

    public class AtomContext<T> : AtomContext
    {
        public T Input { get; }

        internal AtomContext(IDependencyScope scope, T input) : base(scope)
        {
            Input = input;
        }
    }
}