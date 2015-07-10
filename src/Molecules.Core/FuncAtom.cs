using System;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class FuncAtom<T> : Atom<T>
    {
        readonly Func<AtomContext, Task<T>> _impl;

        internal FuncAtom(Func<AtomContext, Task<T>> impl)
        {
            _impl = impl;
        }

        internal override Task<T> ChargeCore(AtomContext context)
        {
            return _impl(context);
        }
    }

    public class FuncAtom<TIn, TOut> : Atom<TOut>
    {
        readonly Func<AtomContext<TIn>, Task<TOut>> _impl;

        internal FuncAtom(Func<AtomContext<TIn>, Task<TOut>> impl)
        {
            _impl = impl;
        }

        internal override Task<TOut> ChargeCore(AtomContext context)
        {
            return _impl((AtomContext<TIn>) context);
        }
    }

    public static partial class Atom
    {
        public static FuncAtom<TIn, TOut> Func<TIn, TOut>(Func<AtomContext<TIn>, Task<TOut>> impl)
        {
            return new FuncAtom<TIn, TOut>(impl);
        }

        public static FuncAtom<TIn, TOut> Func<TIn, TOut>(Func<AtomContext<TIn>, TOut> impl)
        {
            return Func<TIn, TOut>(i => Task.FromResult(impl(i)));
        }

        public static FuncAtom<TOut> Func<TOut>(Func<AtomContext, Task<TOut>> impl)
        {
            return new FuncAtom<TOut>(impl);
        }

        public static FuncAtom<TOut> Func<TOut>(Func<AtomContext, TOut> impl)
        {
            return new FuncAtom<TOut>(c => Task.FromResult(impl(c)));
        }

        public static FuncAtom<TOut> Func<TOut>(Func<Task<TOut>> impl)
        {
            return new FuncAtom<TOut>(_ => impl());
        }
                
        public static Atom<T> Func<T>(Func<T> impl)
        {
            return Func(() => Task.FromResult(impl()));
        }

        public static FuncAtom<T, Unit> Func<T>(Func<AtomContext<T>, Task> impl)
        {
            Func<AtomContext<T>, Task<Unit>> wrapper = async i =>
            {
                await impl(i);
                return Unit.Value;
            };

            return Func(wrapper);
        }

        static FuncAtom<Unit> Action(Func<Task<Unit>> impl)
        {
            return new FuncAtom<Unit>(_ => impl());
        }

        public static FuncAtom<Unit> Action(Func<Task> impl)
        {
            return Action(async () =>
            {
                await impl();
                return Unit.Value;
            });
        }

        public static FuncAtom<Unit> Action(Action body)
        {
            return Action(() => 
            {
                body();
                return Unit.CompletedTask;
            });
        }
        
        public static FuncAtom<T, Unit> Action<T>(Action<AtomContext<T>> body)
        {
            Func<AtomContext<T>, Task<Unit>> wrapper = i =>
            {
                body(i);
                return Unit.CompletedTask;
            };

            return Func(wrapper);
        }
    }
}