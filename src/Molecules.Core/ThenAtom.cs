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

        internal override async Task<TSecond> ChargeCore(AtomContext context, object input = null)
        {
            var i = await First.ChargeCore(context, input);
            return await Second.ChargeCore(context, i);
        }
    }

    public static partial class Atom
    {
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Expression<Func<TOut>> second)
        {
            return first.Then(Of(second));
        }

        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Expression<Func<TIn, TOut>> second)
        {
            return first.Then(Of(second));
        }
        
        public static ThenAtom<TIn, TOut> Then<TIn, TOut>(
            this Atom<TIn> first,
            Atom<TOut> second)
        {
            return new ThenAtom<TIn, TOut>(first, second);
        }

        public static ThenAtom<T, Unit> Then<T>(
            this Atom<T> first,
            Expression<Action<T>> second)
        {
            return first.Then(Of(second));
        }

        public static ThenAtom<T, Unit> Then<T>(
            this Atom<T> first,
            Expression<Action> second)
        {
            return first.Then(Of(second));
        }

        public static Atom<TOut> Select<TIn, TOut>(this Atom<TIn> atom, 
            Expression<Func<TIn, TOut>> projector)
        {
            return atom.Then(projector);
        }
    }
}