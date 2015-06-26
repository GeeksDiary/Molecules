using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dependable.Core
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

        protected override async Task<IEnumerable<T>> OnCharge(object input = null)
        {
            var results = new T[Count];

            for (var i = 0; i < Count; i++)            
                results[i] = await Source.Charge(input);
            
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