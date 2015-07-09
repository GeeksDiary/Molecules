using System.Threading.Tasks;
using Molecules.Core.Runtime;

namespace Molecules.Core
{
    public class InvocableAtom<T> : Atom<T>
    {
        public Atom<T> Target { get; }

        public InvocableAtom(Atom<T> target)
        {
            Target = target;
        }

        public async Task<T> Charge()
        {
            return await MoleculesHost
                .Configuration
                .Processor
                .Process(Target, AtomContext.For(Unit.Value));
        }

        internal override Task<T> ChargeCore(IAtomContext input1)
        {
            throw new System.NotImplementedException();
        }
    }

    public static partial class Atom
    {
        public static InvocableAtom<T> AsInvocable<T>(this Atom<T> target)
        {
            return new InvocableAtom<T>(target);
        }
    }
}