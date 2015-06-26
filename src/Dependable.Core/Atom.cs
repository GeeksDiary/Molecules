using System.Threading.Tasks;

namespace Dependable.Core
{
    public abstract class Atom<T>
    {
        public async Task<T> Charge(object input = null)
        {
            return await OnCharge(input);
        }

        protected abstract Task<T> OnCharge(object input = null);
    }        
}