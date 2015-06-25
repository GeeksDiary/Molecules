using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class ConditionAtom<TSource, TOut> : Atom<TOut>
    {
        readonly Atom<TSource> _source;
        readonly Predicate<TSource> _predicate;
        readonly Atom<TOut> _truthy;
        readonly Atom<TOut> _falsey;

        public ConditionAtom(Atom<TSource> source,
            Predicate<TSource> predicate,
            Atom<TOut> truthy,
            Atom<TOut> falsey)
        {
            _source = source;
            _predicate = predicate;
            _truthy = truthy;
            _falsey = falsey;
        }

        public async override Task<TOut> Charge(object input = null)
        {
            var i = await _source.Charge(input);
            var next = _predicate(i) ? _truthy : _falsey;
            return await next.Charge(i);
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
            Func<TSource, TOut> truthy,
            Func<TSource, TOut> falsey)
        {
            return If(source, predicate, Of(truthy), Of(falsey));
        }

        public static Atom<TOut> If<TSource, TOut>(this Atom<TSource> source,
            Predicate<TSource> predicate,
            Func<TOut> truthy,
            Func<TOut> falsey)
        {
            return If(source, predicate, Of(truthy), Of(falsey));
        }
    }
}