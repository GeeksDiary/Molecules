using System.Threading.Tasks;
using Molecules.Core.Runtime;

namespace Molecules.Core
{
    public class InvokerAtom<T> : Atom<T>
    {
        public Atom<T> Target { get; }

        public InvokerAtom(Atom<T> target)
        {
            Target = target;
        }

        public async Task<T> Charge()
        {
            using (var scope = MoleculesHost.Configuration.DependencyResolver.BeginScope())
            {
                return await MoleculesHost
                    .Configuration
                    .Processor
                    .Process(Target, new AtomContext(scope));
            }                
        }

        internal override Task<T> ChargeCore(AtomContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    public static partial class Atom
    {
        public static InvokerAtom<T> Invoker<T>(this Atom<T> target)
        {
            return new InvokerAtom<T>(target);
        }
    }
}