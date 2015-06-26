using System.Threading.Tasks;

namespace Dependable.Core
{
    public abstract class Atom<T>
    {        
        public abstract Task<T> Charge(object input = null);
    }        
}