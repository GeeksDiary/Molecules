using System;

namespace Molecules.Core
{
    public interface IAtomContext
    {
        T Resolve<T>();
    }

    public interface IAtomContext<out T> : IAtomContext
    {
        T Input { get; }
    }

    internal class AtomContext : IAtomContext
    {
        internal object InputObject { get; }

        protected AtomContext(object inputObject)
        {
            InputObject = inputObject;
        }

        public static AtomContext For(object value)
        {
            return new AtomContext(value);
        }

        public static AtomContext<T> For<T>(T value)
        {
            return new AtomContext<T>(value);
        }

        public T Resolve<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }

    internal class AtomContext<T> : AtomContext, IAtomContext<T>
    {
        public T Input => (T) InputObject;

        internal AtomContext(T value) : base(value)
        {
        }
    }
}