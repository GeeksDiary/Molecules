using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class SelectManyAtom<TFirst, TSecond, TOut> : Atom<TOut>
    {
        readonly Atom<TFirst> _first;
        readonly Func<TFirst, Atom<TSecond>> _second;
        readonly Func<TFirst, TSecond, TOut> _projector;

        public SelectManyAtom(Atom<TFirst> first, 
            Func<TFirst, Atom<TSecond>> second, 
            Func<TFirst, TSecond, TOut> projector)
        {
            _first = first;
            _second = second;
            _projector = projector;
        }
        
        public override async Task<TOut> Charge(object input = null)
        {
            var first = await _first.Charge(input);
            var second = await _second(first).Charge();
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