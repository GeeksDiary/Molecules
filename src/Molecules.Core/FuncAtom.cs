using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Molecules.Core
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

        internal override Task<T> ChargeCore(AtomContext context, object input = null)
        {
            return _impl(input);
        }
    }

    public class FuncAtom<TIn, TOut> : FuncAtom<TOut>
    {
        internal FuncAtom(Func<TIn, Task<TOut>> impl, Expression body) :
            base(o => impl((TIn)o), body)
        {
        }
    }

    public static partial class Atom
    {
        static FuncAtom<TIn, TOut> Of<TIn, TOut>(Func<TIn, Task<TOut>> impl, Expression body)
        {
            return new FuncAtom<TIn, TOut>(impl, body);
        }

        static FuncAtom<TOut> Of<TOut>(Func<Task<TOut>> impl, Expression body)
        {
            return new FuncAtom<TOut>(_ => impl(), body);
        }

        public static FuncAtom<TIn, TOut> Of<TIn, TOut>(Expression<Func<TIn, Task<TOut>>> body)
        {
            var compiled = body.Compile();
            return Of(compiled, body);
        }

        public static FuncAtom<TIn, TOut> Of<TIn, TOut>(Expression<Func<TIn, TOut>> body)
        {
            var compiled = body.Compile();
            return Of<TIn, TOut>(i => Task.FromResult(compiled(i)), body);
        }

        public static Atom<T> Of<T>(Expression<Func<T>> body)
        {
            var compiled = body.Compile();
            return Of(() => Task.FromResult(compiled()), body);
        }

        public static Atom<T> Of<T>(Expression<Func<Task<T>>> body)
        {
            var compiled = body.Compile();
            return Of(compiled, body);
        }
    }
}