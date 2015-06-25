using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class SelectManyAtom<TFirst, TSecond, TOut> : Atom<TOut>
    {
        public Atom<TFirst> First { get; }

        public Func<TFirst, Atom<TSecond>> Second { get; }

        public Func<TFirst, TSecond, TOut> Projector { get; }

        public SelectManyAtom(Atom<TFirst> first, 
            Func<TFirst, Atom<TSecond>> second, 
            Func<TFirst, TSecond, TOut> projector)
        {
            First = first;
            Second = second;
            Projector = projector;
        }
        
        public override async Task<TOut> Charge(object input = null)
        {
            var first = await First.Charge(input);
            var second = await Second(first).Charge();
            return Projector(first, second);
        }
    }

    public static partial class Atom
    {
        public static Atom<TOut> SelectMany<TFirst, TSecond, TOut>(
            this Atom<TFirst> first,
            Func<TFirst, Atom<TSecond>> selector,
            Func<TFirst, TSecond, TOut> projector
            )
        {
            return new SelectManyAtom<TFirst, TSecond, TOut>(first, selector, projector);
        }
    }
}