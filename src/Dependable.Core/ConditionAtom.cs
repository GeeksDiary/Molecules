using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public abstract class ConditionAtom<TIn, TIntermediary, TOut> : Atom<TIn, TOut>
    {
        readonly Atom<TIn, TIntermediary> _source;
        readonly Predicate<TIntermediary> _predicate;

        protected ConditionAtom(Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        public async override Task<TOut> Charge(TIn input)
        {
            var i = await _source.Charge(input);
            return await (_predicate(i) ? OnTruthy(i) : OnFalsey(i));
        }

        protected abstract Task<TOut> OnTruthy(TIntermediary intermediary);

        protected abstract Task<TOut> OnFalsey(TIntermediary intermediary);
    }

    public class ActionAtomCondition<TIn, TIntermediary> : 
        ConditionAtom<TIn, TIntermediary, Unit>
    {
        readonly ActionAtom _truthy;
        readonly ActionAtom _falsey;

        public ActionAtomCondition(Atom<TIn, 
            TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            ActionAtom truthy,
            ActionAtom falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected override async Task<Unit> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge();
        }

        protected override async Task<Unit> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge();
        }
    }

    public class UnaryActionAtomCondition<TIn, TIntermediary> : 
        ConditionAtom<TIn, TIntermediary, Unit>
    {
        readonly UnaryActionAtom<TIntermediary> _truthy;
        readonly UnaryActionAtom<TIntermediary> _falsey;

        public UnaryActionAtomCondition(
            Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            UnaryActionAtom<TIntermediary> truthy,
            UnaryActionAtom<TIntermediary> falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected async override Task<Unit> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge(intermediary);
        }

        protected async override Task<Unit> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge(intermediary);
        }
    }

    public class NullaryFuncAtomJuction<TIn, TIntermediary, TOut> : 
        ConditionAtom<TIn, TIntermediary, TOut>
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

    public class UnaryFuncAtomCondition<TIn, TIntermediary, TOut> : 
        ConditionAtom<TIn, TIntermediary, TOut>
    {
        readonly Atom<TIntermediary, TOut> _truthy;
        readonly Atom<TIntermediary, TOut> _falsey;

        public UnaryFuncAtomCondition(Atom<TIn, TIntermediary> source, 
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
    
    public static partial class Atom
    {
        public static ConditionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Atom<TIntermediary, TOut> truthy,
            Atom<TIntermediary, TOut> falsey)
        {
            return new UnaryFuncAtomCondition<TIn, TIntermediary, TOut>(source, predicate, truthy, falsey);
        }

        public static ConditionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Func<TOut> truthy,
            Func<TOut> falsey)
        {
            return source.If(predicate, Of(truthy), Of(falsey));
        }

        public static ConditionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            NullaryFuncAtom<TOut> truthy,
            NullaryFuncAtom<TOut> falsey)
        {
            return new NullaryFuncAtomJuction<TIn, TIntermediary, TOut>(source, predicate, truthy, falsey);
        }

        public static UnaryActionAtomCondition<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Action<TIntermediary> truthy,
            Action<TIntermediary> falsey)
        {
            return source.If(predicate, Of(truthy), Of(falsey));
        }

        public static UnaryActionAtomCondition<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            UnaryActionAtom<TIntermediary> truthy,
            UnaryActionAtom<TIntermediary> falsey)
        {
            return new UnaryActionAtomCondition<TIn, TIntermediary>(source, predicate, truthy, falsey);
        }

        public static ActionAtomCondition<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Action truthy,
            Action falsey)
        {
            return source.If(predicate, Of(truthy), Of(falsey));
        }

        public static ActionAtomCondition<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            ActionAtom truthy,
            ActionAtom falsey
            )
        {
            return new ActionAtomCondition<TIn, TIntermediary>(source, predicate, truthy, falsey);
        }
    }
}