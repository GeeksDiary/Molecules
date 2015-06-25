using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class Atom<T>
    {
        readonly Func<object, Task<T>> _impl;

        internal Atom()
        {            
        }

        internal Atom(Func<object, Task<T>> impl)
        {
            _impl = impl;
        }

        public virtual Task<T> Charge(object input = null)
        {
            return _impl(input);
        }
    }
    
    public static partial class Atom
    {
        public static Atom<TOut> From<TIn, TOut>(Func<TIn, TOut> impl)
        {
            return From<TIn, TOut>(i => Task.FromResult(impl(i)));
        }

        public static Atom<T> From<T>(Func<T> impl)
        {
            return From(() => Task.FromResult(impl()));
        }

        public static Atom<T> From<T>(Func<Task<T>> impl)
        {
            return new Atom<T>(_ => impl());
        }

        public static Atom<TOut> From<TIn, TOut>(Func<TIn, Task<TOut>> impl)
        {
            return new Atom<TOut>(i => impl((TIn)i));
        }
    }    
}