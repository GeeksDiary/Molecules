using System;
using System.Threading.Tasks;
using Molecules.Core.Utilities;

namespace Molecules.Core
{
    public class CatchAtom<T> : Atom<T>
    {
        public Atom<T> Source { get; }

        public int RetryCount { get; private set; }

        public TimeSpan Delay { get; private set; }

        public Func<T> ReturnValue { get; private set; }

        public CatchAtom(Atom<T> source)
        {
            Source = source;
        }        

        public CatchAtom<T> Wait(TimeSpan delay)
        {
            return new CatchAtom<T> (Source)
            {
                Delay = delay,
                RetryCount = RetryCount,
                ReturnValue = ReturnValue
            };            
        }

        public CatchAtom<T> Retry(int count)
        {
            return new CatchAtom<T>(Source)
            {
                RetryCount = count,
                Delay = Delay,
                ReturnValue = ReturnValue
            };
        }

        public TimeSpanBuilder<CatchAtom<T>> Wait(int size)
        {
            return new TimeSpanBuilder<CatchAtom<T>>(size, Wait);
        }

        public CatchAtom<T> Return(T value)
        {
            return new CatchAtom<T>(Source)
            {
                ReturnValue = () => value,
                Delay = Delay,
                RetryCount = RetryCount
            };
        }

        internal async override Task<T> ChargeCore(AtomContext context)
        {
            var remainingAttempts = RetryCount + 1;

            while (true)
            {
                try
                {
                    return await Source.ChargeCore(context);
                }
                catch (Exception e)
                {
                    if (e.IsFatal())
                        throw;

                    if (--remainingAttempts == 0)
                    {
                        if (ReturnValue == null)
                            throw;
                        return ReturnValue();
                    }
                }

                if (Delay != TimeSpan.Zero)
                    await Task.Delay(Delay);
            }
        }
    }

    public static partial class Atom
    {
        public static CatchAtom<T> Catch<T>(this Atom<T> source)
        {
            var result = source as CatchAtom<T>;
            return result == null ? new CatchAtom<T>(source) 
                : new CatchAtom<T>(result.Source);
        }
    }
}