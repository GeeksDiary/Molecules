using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class ThenAtom<TFirst, TSecond> : Atom<TSecond>
    {
        readonly Atom<TFirst> _first;
        readonly Atom<TSecond> _second;

        public ThenAtom(Atom<TFirst> first, Atom<TSecond> second)
        {
            _first = first;
            _second = second;
        }

        public override async Task<TSecond> Charge(object input = null)
        {
            var i = await _first.Charge(input);
            return await _second.Charge(i);
        }
    }

    public static partial class Atom
    {
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Func<TOut> second)
        {
            return first.Then(From(second));
        }

        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Func<TIn, TOut> second)
        {
            return first.Then(From(second));
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