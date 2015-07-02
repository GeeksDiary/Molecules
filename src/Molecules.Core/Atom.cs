using System.Threading.Tasks;

namespace Molecules.Core
{
    public abstract class Atom<T>
    {
        internal async Task<T> ChargeCore(object input = null)
        {
            return await OnCharge(input);
        }

        protected abstract Task<T> OnCharge(object input = null);
    }
}