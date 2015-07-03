using System;
using System.Threading.Tasks;
using Molecules.Core.Utilities;

namespace Molecules.Core
{
    public class RetryAtom<T> : Atom<T>
    {
        public Atom<T> Source { get; }

        public int Count { get; }

        public TimeSpan Delay { get; private set; }

        public RetryAtom(Atom<T> source, int count)
        {
            Source = source;
            Count = count;
        }

        public RetryAtom<T> After(TimeSpan delay)
        {
            Delay = delay;
            return this;
        }

        protected async override Task<T> OnCharge(object input = null)
        {
            var remainingAttempts = Count + 1;

            while (true)
            {
                try
                {
                    return await Source.ChargeCore(input);
                }
                catch (Exception e)
                {
                    if (e.IsFatal())
                        throw;

                    if (--remainingAttempts == 0)
                        throw;
                    
                }

                if (Delay != TimeSpan.Zero)
                    await Task.Delay(Delay);
            }
        }
    }

    public static partial class Atom
    {
        public static RetryAtom<T> Retry<T>(this Atom<T> source, int count)
        {
            var result = source as RetryAtom<T>;
            return result == null ? new RetryAtom<T>(source, count) 
                : new RetryAtom<T>(result.Source, count);
        }
    }
}