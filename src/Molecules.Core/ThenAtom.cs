using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Molecules.Core
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

        internal override async Task<TSecond> ChargeCore(AtomContext atomContext)
        {
            var i = await First.ChargeCore(atomContext);
            return await Second.ChargeCore(AtomContext.For(i));
        }
    }

    public static partial class Atom
    {
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(this Atom<TIn> first,
            Func<TOut> second)
        {
            return first.Then(Func(second));
        }

        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(this Atom<TIn> first,
            Func<AtomContext<TIn>, TOut> second)
        {
            return first.Then(Func(second));
        }
        
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(this Atom<TIn> first,
            Atom<TOut> second)
        {
            return new ThenAtom<TIn, TOut>(first, second);
        }

        public static ThenAtom<T, Unit> Then<T>(this Atom<T> first,
            Action<AtomContext<T>> second)
        {
            return first.Then(Action(second));
        }

        public static ThenAtom<T, Unit> Then<T>(
            this Atom<T> first,
            Action second)
        {
            return first.Then(Action(second));
        }

        public static Atom<TOut> Select<TIn, TOut>(this Atom<TIn> atom, 
            Func<AtomContext<TIn>, TOut> projector)
        {
            return atom.Then(projector);
        }
    }
}