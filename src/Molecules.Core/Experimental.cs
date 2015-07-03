using System;
using System.Threading.Tasks;
using Molecules.Core.Utilities;

namespace Molecules.Core
{
    public class WithAtom<T> : Atom<T>
    {
        protected override Task<T> OnCharge(object input = null)
        {
            return Task.FromResult((T) input);
        }
    }

    public class ReturnAtom<T> : Atom<T>
    {
        public T Value { get; set; }

        public ReturnAtom(T value)
        {
            Value = value;
        }

        protected override Task<T> OnCharge(object input = null)
        {
            return Task.FromResult(Value);
        }
    }


    public static partial class Atom
    {
        public static Atom<T> With<T>()
        {
            return new WithAtom<T>();
        }

        public static Atom<T> AsAtom<T>(this Atom<T> source)
        {
            return source;
        }

        public static Atom<T> Return<TFirst, T>(this Atom<TFirst> source, T value)
        {
            return source.Then(new ReturnAtom<T>(value));
        }

        public static Atom<T> Return<T>(T value)
        {
            return new ReturnAtom<T>(value);
        }
    }

    public class TimeSpanBuilder<T>
    {
        readonly int _size;
        readonly Func<TimeSpan, T> _innerBuilder;

        public TimeSpanBuilder(int size, Func<TimeSpan, T> innerBuilder)
        {
            _size = size;
            _innerBuilder = innerBuilder;
        }

        public T Minutes => _innerBuilder(TimeSpan.FromMinutes(_size));

        public T Seconds => _innerBuilder(TimeSpan.FromSeconds(_size));

        public T Hours => _innerBuilder(TimeSpan.FromHours(_size));

        public T MilliSeconds => _innerBuilder(TimeSpan.FromMilliseconds(_size));
    }
}