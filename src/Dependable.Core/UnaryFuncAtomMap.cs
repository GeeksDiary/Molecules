using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class MapAtom<TSource, TOut> : Atom<IEnumerable<TOut>>
    {
        readonly Atom<IEnumerable<TSource>> _source;
        readonly Atom<TOut> _map;

        public MapAtom(Atom<IEnumerable<TSource>> source, Atom<TOut> map)
        {
            _source = source;
            _map = map;
        }

        public async override Task<IEnumerable<TOut>> Charge(object input = null)
        {
            var d = await _source.Charge(input);
            return await Task.WhenAll(d.Select(i => _map.Charge(i)));
        }
    }
    
    public static partial class Atom
    {
        public static MapAtom<TSource, TOut> Map<TSource, TOut>(
            this Atom<IEnumerable<TSource>> source,
            Atom<TOut> map)
        {
            return new MapAtom<TSource, TOut>(source, map);
        }

        public static MapAtom<TSource, TOut> Map<TSource, TOut>(
            this Atom<IEnumerable<TSource>> source,
            Func<TSource, TOut> map)
        {
            return Map(source, Of(map));
        }

        public static MapAtom<TSource, TOut> Map<TSource, TOut>(
            this Atom<IEnumerable<TSource>> source,
            Func<TOut> map)
        {
            return Map(source, Of(map));
        }
    }
}