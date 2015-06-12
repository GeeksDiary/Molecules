using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class ActionAtomJunction<TIn, TIntermediary> : 
        JunctionAtom<TIn, TIntermediary, Value>
    {
        readonly ActionAtom _truthy;
        readonly ActionAtom _falsey;

        public ActionAtomJunction(Atom<TIn, 
            TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            ActionAtom truthy,
            ActionAtom falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected override async Task<Value> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge();
        }

        protected override async Task<Value> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge();
        }
    }

    public class UnaryActionAtomJunction<TIn, TIntermediary> : 
        JunctionAtom<TIn, TIntermediary, Value>
    {
        readonly UnaryActionAtom<TIntermediary> _truthy;
        readonly UnaryActionAtom<TIntermediary> _falsey;

        public UnaryActionAtomJunction(
            Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            UnaryActionAtom<TIntermediary> truthy,
            UnaryActionAtom<TIntermediary> falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected async override Task<Value> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge(intermediary);
        }

        protected async override Task<Value> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge(intermediary);
        }
    }

    public class NullaryFuncAtomJuction<TIn, TIntermediary, TOut> : 
        JunctionAtom<TIn, TIntermediary, TOut>
    {
        readonly NullaryFuncAtom<TOut> _truthy;
        readonly NullaryFuncAtom<TOut> _falsey;

        public NullaryFuncAtomJuction(
            Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            NullaryFuncAtom<TOut> truthy,
            NullaryFuncAtom<TOut> falsey) : 
                base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected async override Task<TOut> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge();
        }

        protected override async Task<TOut> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge();
        }
    }

    public class UnaryFuncAtomJunction<TIn, TIntermediary, TOut> : 
        JunctionAtom<TIn, TIntermediary, TOut>
    {
        readonly Atom<TIntermediary, TOut> _truthy;
        readonly Atom<TIntermediary, TOut> _falsey;

        public UnaryFuncAtomJunction(Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate, 
            Atom<TIntermediary, TOut> truthy, 
            Atom<TIntermediary, TOut> falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected async override Task<TOut> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge(intermediary);
        }

        protected async override Task<TOut> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge(intermediary);
        }
    }

    public abstract class JunctionAtom<TIn, TIntermediary, TOut> : Atom<TIn, TOut>
    {
        readonly Atom<TIn, TIntermediary> _source;
        readonly Predicate<TIntermediary> _predicate;

        protected JunctionAtom(Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        public async override Task<TOut> Charge(TIn input)
        {
            var i = await _source.Charge(input);
            return await(_predicate(i) ? OnTruthy(i) : OnFalsey(i));
        }

        protected abstract Task<TOut> OnTruthy(TIntermediary intermediary);

        protected abstract Task<TOut> OnFalsey(TIntermediary intermediary);
    }
}