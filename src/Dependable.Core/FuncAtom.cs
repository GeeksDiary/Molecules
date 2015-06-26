using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class FuncAtom<T> : Atom<T>
    {
        readonly Func<object, Task<T>> _impl;

        public Expression Body { get; }

        internal FuncAtom(Func<object, Task<T>> impl, Expression body)
        {
            _impl = impl;
            Body = body;
        }

        protected override Task<T> OnCharge(object input = null)
        {
            return _impl(input);
        }
    }

    public static partial class Atom
    {
        static Atom<TOut> Of<TIn, TOut>(Func<TIn, Task<TOut>> impl, Expression body)
        {
            return new FuncAtom<TOut>(i => impl((TIn)i), body);
        }

        public static Atom<TOut> Of<TIn, TOut>(Expression<Func<TIn, Task<TOut>>> body)
        {
            var compiled = body.Compile();
            return Of(compiled, body);
        }

        public static Atom<TOut> Of<TIn, TOut>(Expression<Func<TIn, TOut>> body)
        {
            var compiled = body.Compile();
            return Of<TIn, TOut>(i => Task.FromResult(compiled(i)), body);
        }

        public static Atom<T> Of<T>(Expression<Func<T>> body)
        {
            var compiled = body.Compile();
            return Of<object, T>(_ => Task.FromResult(compiled()), body);
        }

        public static Atom<T> Of<T>(Expression<Func<Task<T>>> body)
        {
            var compiled = body.Compile();
            return Of<object, T>(_ => compiled(), body);
        }
    }
}