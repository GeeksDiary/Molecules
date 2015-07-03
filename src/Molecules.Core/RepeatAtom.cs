using System.Collections.Generic;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class RepeatAtom<T> : Atom<IEnumerable<T>>
    {
        public Atom<T> Source { get; }

        public int Count { get; }

        public RepeatAtom(Atom<T> source, int count)
        {
            Source = source;
            Count = count;
        }

        internal override async Task<IEnumerable<T>> ChargeCore(AtomContext context, object input = null)
        {
            var results = new T[Count];

            for (var i = 0; i < Count; i++)            
                results[i] = await Source.ChargeCore(context, input);
            
            return results;
        }
    }

    public static partial class Atom
    {
        public static RepeatAtom<T> Repeat<T>(this Atom<T> source, int count)
        {
            return new RepeatAtom<T>(source, count);
        }
    }
}