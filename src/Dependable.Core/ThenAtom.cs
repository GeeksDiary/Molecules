using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class ThenAtom<TFirst, TSecond> : Atom<TSecond>
    {
        public Atom<TFirst> First { get; }

        public Atom<TSecond> Second { get; }

        public ThenAtom(Atom<TFirst> first, Atom<TSecond> second)
        {
            First = first;
            Second = second;
        }

        public override async Task<TSecond> Charge(object input = null)
        {
            var i = await First.Charge(input);
            return await Second.Charge(i);
        }
    }

    public static partial class Atom
    {
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Func<TOut> second)
        {
            return first.Then(Of(second));
        }

        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Func<TIn, TOut> second)
        {
            return first.Then(Of(second));
        }
        
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Atom<TOut> second)
        {
            return new ThenAtom<TIn, TOut>(first, second);
        }
        
        public static Atom<TOut> Select<TIn, TOut>(this Atom<TIn> atom, Func<TIn, TOut> projector)
        {
            return atom.Then(projector);
        }
    }
}