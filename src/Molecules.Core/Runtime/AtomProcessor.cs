using System;
using System.Threading.Tasks;

namespace Molecules.Core.Runtime
{
    public abstract class AtomProcessor
    {
        public AtomProcessor Next { get; }

        protected AtomProcessor(AtomProcessor next)
        {
            Next = next;
        }

        public abstract Task<T> Process<T>(Atom<T> atom, AtomContext context);
    }
}