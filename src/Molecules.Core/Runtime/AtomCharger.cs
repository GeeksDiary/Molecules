using System;
using System.Threading.Tasks;

namespace Molecules.Core.Runtime
{
    public abstract class AtomCharger
    {
        public AtomCharger Next { get; }

        protected AtomCharger(AtomCharger next)
        {
            Next = next;
        }

        public abstract Task<T> Run<T>(Atom<T> atom, AtomContext context);
    }
}