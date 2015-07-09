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

        internal override async Task<TSecond> ChargeCore(IAtomContext context)
        {
            var i = await First.ChargeCore(context);
            return await Second.ChargeCore(context.Clone(i));
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
            Func<IAtomContext<TIn>, TOut> second)
        {
            return first.Then(Func(second));
        }
        
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(this Atom<TIn> first,
            Atom<TOut> second)
        {
            return new ThenAtom<TIn, TOut>(first, second);
        }

        public static ThenAtom<T, Unit> Then<T>(this Atom<T> first,
            Action<IAtomContext<T>> second)
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
            Func<IAtomContext<TIn>, TOut> projector)
        {
            return atom.Then(projector);
        }
    }
}