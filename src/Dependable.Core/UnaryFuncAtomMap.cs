using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class UnaryFuncAtomMap<TIn, TIntermediary, TOut> : Atom<TIn, IEnumerable<TOut>>
    {
        readonly Atom<TIn, IEnumerable<TIntermediary>> _source;
        readonly Atom<TIntermediary, TOut> _map;

        public UnaryFuncAtomMap(Atom<TIn, IEnumerable<TIntermediary>> source, Atom<TIntermediary, TOut> map)
        {
            _source = source;
            _map = map;
        }

        public async override Task<IEnumerable<TOut>> Charge(TIn input)
        {
            var d = await _source.Charge(input);
            return await Task.WhenAll(d.Select(i => _map.Charge(i)));
        }
    }

    public class NullaryFuncAtomMap<TIntermediary, TOut> : UnaryFuncAtomMap<Unit, TIntermediary, TOut>
    {
        public NullaryFuncAtomMap(NullaryFuncAtom<IEnumerable<TIntermediary>> source, Atom<TIntermediary, TOut> map) :
            base(source, map)
        {
        }

        public async Task<IEnumerable<TOut>> Charge()
        {
            return await base.Charge(Unit.None);
        }
    }


    public static partial class Atom
    {
        public static UnaryFuncAtomMap<TIn, TIntermediary, TOut> Map<TIn, TIntermediary, TOut>(
            this Atom<TIn, IEnumerable<TIntermediary>> source,
            Func<TIntermediary, TOut> map)
        {
            return new UnaryFuncAtomMap<TIn, TIntermediary, TOut>(source, Of(map));
        }

        public static NullaryFuncAtomMap<TIntermediary, TOut> Map<TIntermediary, TOut>(
            this NullaryFuncAtom<IEnumerable<TIntermediary>> source,
            Func<TIntermediary, TOut> map)
        {
            return new NullaryFuncAtomMap<TIntermediary, TOut>(source, Of(map));
        }
    }
}