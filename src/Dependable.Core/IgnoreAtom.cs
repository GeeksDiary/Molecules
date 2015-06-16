using System.Threading.Tasks;

namespace Dependable.Core
{
    public class IgnoreAtom<TIn, TOut> : Atom<TIn, Value>
    {
        readonly Atom<TIn, TOut> _source;

        internal IgnoreAtom(Atom<TIn, TOut> source)
        {
            _source = source;
        }

        public async override Task<Value> Charge(TIn input)
        {
            await _source.Charge(input);
            return await Task.FromResult(Value.None);
        }
    }

    public static partial class Atom
    {
        public static IgnoreAtom<TIn, TOut> Ignore<TIn, TOut>(this Atom<TIn, TOut> source)
        {
            return new IgnoreAtom<TIn, TOut>(source);
        }
    }
}