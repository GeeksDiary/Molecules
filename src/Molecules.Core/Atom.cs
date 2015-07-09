using System.Threading.Tasks;

namespace Molecules.Core
{
    public abstract class Atom<T>
    {
        internal abstract Task<T> ChargeCore(IAtomContext context);
    }
}