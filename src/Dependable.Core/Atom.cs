using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public abstract class Atom<TIn, TOut>
    {
        public abstract Task<TOut> Charge(TIn input);
    }

    public class UnaryFuncAtom<TIn, TOut> : Atom<TIn, TOut>
    {
        readonly Func<TIn, Task<TOut>> _impl;

        internal UnaryFuncAtom(Func<TIn, Task<TOut>> impl)
        {
            _impl = impl;
        }

        public override Task<TOut> Charge(TIn input)
        {
            return _impl(input);
        }
    }

    public class ActionAtom : UnaryFuncAtom<Value, Value>
    {
        internal ActionAtom(Func<Value, Task<Value>> impl) : base(impl)
        {
        }

        public async Task<Value> Charge()
        {
            return await base.Charge(Value.None);
        }
    }

    public class UnaryActionAtom<TIn> : UnaryFuncAtom<TIn, Value>
    {
        internal UnaryActionAtom(Func<TIn, Task<Value>> impl) : base(impl)
        {
        }
    }

    public class NullaryFuncAtom<TOut> : UnaryFuncAtom<Value, TOut>
    {
        internal NullaryFuncAtom(Func<Task<TOut>> impl) : base(v => impl())
        {
        }

        public async Task<TOut> Charge()
        {
            return await base.Charge(Value.None);
        }
    }
    
    public static partial class Atom
    {
        public static UnaryFuncAtom<TIn, TOut> Of<TIn, TOut>(Func<TIn, TOut> impl)
        {
            return new UnaryFuncAtom<TIn, TOut>(i => Task.FromResult(impl(i)));
        }

        public static NullaryFuncAtom<TOut> Of<TOut>(Func<TOut> impl)
        {
            return new NullaryFuncAtom<TOut>(() => Task.FromResult(impl()));
        }

        public static UnaryActionAtom<TIn> Of<TIn>(Action<TIn> impl)
        {
            return new UnaryActionAtom<TIn>(i =>
            {
                impl(i);
                return Value.CompletedNone;
            });
        }

        public static ActionAtom Of(Action impl)
        {
            return new ActionAtom(i => {
                impl();
                return Value.CompletedNone;
            });
        }         
    }
}