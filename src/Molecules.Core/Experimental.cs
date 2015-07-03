using System.Threading.Tasks;

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
    }
}