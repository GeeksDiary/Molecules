using Molecules.Core.Depenencies;

namespace Molecules.Core
{
    public interface IAtomContext
    {
        T Resolve<T>();

        IAtomContext Clone();

        IAtomContext<T> Clone<T>(T input);
    }

    public interface IAtomContext<out T> : IAtomContext
    {
        T Input { get; }        
    }

    internal class AtomContext : IAtomContext
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

        public virtual IAtomContext Clone()
        {
            return new AtomContext(_scope);
        }

        public IAtomContext<T> Clone<T>(T input)
        {
            return new AtomContext<T>(_scope, input);
        }
    }

    internal class AtomContext<T> : AtomContext, IAtomContext<T>
    {
        public T Input { get; }

        internal AtomContext(IDependencyScope scope, T input) : base(scope)
        {
            Input = input;
        }
    }
}