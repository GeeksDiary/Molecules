using System;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class SelectManyAtom<TFirst, TSecond, TOut> : Atom<TOut>
    {
        readonly Func<TFirst, Atom<TSecond>> _selector;
        readonly Func<TFirst, TSecond, TOut> _projector;

        public Atom<TFirst> Source { get; }
        
        public SelectManyAtom(Atom<TFirst> source, 
            Func<TFirst, Atom<TSecond>> selector, 
            Func<TFirst, TSecond, TOut> projector)
        {
            Source = source;      
            _selector = selector;
            _projector = projector;
        }

        internal override async Task<TOut> ChargeCore(AtomContext context)
        {
            var first = await Source.ChargeCore(context);
            var second = await _selector(first).ChargeCore(context);
            return _projector(first, second);
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