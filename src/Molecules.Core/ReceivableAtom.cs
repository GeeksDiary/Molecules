using System.Threading.Tasks;

namespace Molecules.Core
{
    public class ReceivableAtom<TIn, TOut> : Atom<TOut>
    {
        public Atom<TOut> Target { get; }

        public ReceivableAtom(Atom<TOut> target)
        {
            Target = target;
        }

        public async Task<TOut> Charge(TIn input)
        {
            return await Target.ChargeCore(input);
        }

        protected override Task<TOut> OnCharge(object input = null)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ReceivableAtomBuilder<T>
    {
        readonly Atom<T> _target;

        public ReceivableAtomBuilder(Atom<T> target)
        {
            _target = target;
        }

        public ReceivableAtom<TIn, T> Of<TIn>()
        {
            return new ReceivableAtom<TIn, T>(_target);
        }
    }

    public static partial class Atom
    {
        public static ReceivableAtomBuilder<T> AsReceivable<T>(this Atom<T> target)
        {
            return new ReceivableAtomBuilder<T>(target);
        }
    }
}