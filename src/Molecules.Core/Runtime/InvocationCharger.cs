using System.Threading.Tasks;

namespace Molecules.Core.Runtime
{
    public class InvocationCharger : AtomCharger
    {
        public InvocationCharger() : base(null)
        {
        }

        public override async Task<T> Run<T>(Atom<T> atom, AtomContext context)
        {
            return await atom.ChargeCore(context);
        }        
    }
}