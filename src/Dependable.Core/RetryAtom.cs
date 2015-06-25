using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class RetryAtom<T> : Atom<T>
    {
        public Atom<T> Source { get; }
        public int Count { get; }

        public RetryAtom(Atom<T> source, int count)
        {
            Source = source;
            Count = count;
        }

        public async override Task<T> Charge(object input = null)
        {
            var remainingAttempts = Count + 1;

            while (true)
            {
                try
                {
                    return await Source.Charge(input);
                }
                catch (Exception)
                {
                    if (--remainingAttempts == 0)
                        throw;
                }
            }            
        }
    }

    public static partial class Atom
    {
        public static RetryAtom<T> Retry<T>(this Atom<T> source, int count = 1)
        {
            var result = source as RetryAtom<T>;
            return result == null ? new RetryAtom<T>(source, count) 
                : new RetryAtom<T>(result.Source, count);
        }
    }
}