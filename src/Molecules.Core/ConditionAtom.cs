using System;
using System.Linq.Expressions;
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

        internal async override Task<TOut> ChargeCore(AtomContext context, object input = null)
        {
            var i = await Source.ChargeCore(context, input);
            var next = Condition(i) ? Truthy : Falsey;
            return await next.ChargeCore(context, i);
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
            Expression<Func<TSource, TOut>> truthy,
            Expression<Func<TSource, TOut>> falsey)
        {
            return If(source, predicate, Of(truthy), Of(falsey));
        }

        public static Atom<TOut> If<TSource, TOut>(this Atom<TSource> source,
            Predicate<TSource> predicate,
            Expression<Func<TOut>> truthy,
            Expression<Func<TOut>> falsey)
        {
            return If(source, predicate, Of(truthy), Of(falsey));
        }
    }
}