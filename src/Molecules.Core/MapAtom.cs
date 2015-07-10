using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class MapAtom<TSource, TOut> : Atom<IEnumerable<TOut>>
    {
        public Atom<IEnumerable<TSource>> Source { get; }

        public Atom<TOut> Map { get; }

        public MapAtom(Atom<IEnumerable<TSource>> source, Atom<TOut> map)
        {
            Source = source;
            Map = map;
        }

        internal async override Task<IEnumerable<TOut>> ChargeCore(AtomContext context)
        {
            var d = await Source.ChargeCore(context);
            return await Task.WhenAll(d.Select(i => Map.ChargeCore(context.Clone(i))));
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
            Func<AtomContext<TSource>, TOut> map)
        {
            return Map(source, Func(map));
        }

        public static MapAtom<TSource, TOut> Map<TSource, TOut>(
            this Atom<IEnumerable<TSource>> source,
            Func<TOut> map)
        {
            return Map(source, Func(map));
        }
    }
}