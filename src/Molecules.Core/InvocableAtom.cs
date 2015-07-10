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
        public static InvocableAtom<T> AsInvocable<T>(this Atom<T> target)
        {
            return new InvocableAtom<T>(target);
        }
    }
}