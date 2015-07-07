using System;

namespace Molecules.Core
{
    public class AtomContext
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

    public class AtomContext<T> : AtomContext
    {
        public T Input => (T)InputObject;

        internal AtomContext(T value) : base(value)
        {
        }
    }

}