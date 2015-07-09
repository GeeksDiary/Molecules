using System.Threading.Tasks;

namespace Molecules.Core.Runtime
{
    public class InvocationProcessor : AtomProcessor
    {
        public InvocationProcessor() : base(null)
        {
        }

        public override async Task<T> Process<T>(Atom<T> atom, AtomContext context)
        {
            return await atom.ChargeCore(context);
        }        
    }
}