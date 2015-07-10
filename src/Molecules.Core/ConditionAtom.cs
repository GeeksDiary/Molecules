using System;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class ConditionAtom<TSource, TOut> : Atom<TOut>
    {
        public Atom<TSource> Source { get; }

        public Predicate<TSource> Condition { get; }

        public Atom<TOut> Truthy { get; }

        public Atom<TOut> Falsey { get; }

        public ConditionAtom(Atom<TSource> source,
            Predicate<TSource> predicate,
            Atom<TOut> truthy,
            Atom<TOut> falsey)
        {
            Source = source;
            Condition = predicate;
            Truthy = truthy;
            Falsey = falsey;
        }

        internal async override Task<TOut> ChargeCore(AtomContext context)
        {
            var i = await Source.ChargeCore(context);
            var next = Condition(i) ? Truthy : Falsey;
            return await next.ChargeCore(context.Clone(i));
        }        
    }

    public static partial class Atom
    {
        public static Atom<TOut> If<TSource, TOut>(this Atom<TSource> source,
            Predicate<TSource> predicate,
            Atom<TOut> truthy,
            Atom<TOut> falsey)
        {
            return new ConditionAtom<TSource, TOut>(source, predicate, truthy, falsey);
        }

        public static Atom<TOut> If<TSource, TOut>(this Atom<TSource> source,
            Predicate<TSource> predicate,
            Func<AtomContext<TSource>, TOut> truthy,
            Func<AtomContext<TSource>, TOut> falsey)
        {
            return If(source, predicate, Func(truthy), Func(falsey));
        }

        public static Atom<TOut> If<TSource, TOut>(this Atom<TSource> source,
            Predicate<TSource> predicate,
            Func<TOut> truthy,
            Func<TOut> falsey)
        {
            return If(source, predicate, Func(truthy), Func(falsey));
        }
    }
}