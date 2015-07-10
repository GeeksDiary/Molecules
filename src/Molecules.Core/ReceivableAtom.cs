using System.Threading.Tasks;
using Molecules.Core.Runtime;

namespace Molecules.Core
{
    public class ReceiverAtom<TIn, TOut> : Atom<TOut>
    {
        public Atom<TOut> Target { get; }

        public ReceiverAtom(Atom<TOut> target)
        {
            Target = target;
        }

        public async Task<TOut> Charge(TIn input)
        {
            using (var scope = MoleculesHost.Configuration.DependencyResolver.BeginScope())
            {
                return await MoleculesHost
                    .Configuration
                    .Processor
                    .Process(Target, new AtomContext<TIn>(scope, input));
            }                
        }

        internal override Task<TOut> ChargeCore(AtomContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ReceiverAtomBuilder<T>
    {
        readonly Atom<T> _target;

        public ReceiverAtomBuilder(Atom<T> target)
        {
            _target = target;
        }

        public ReceiverAtom<TIn, T> Listen<TIn>()
        {
            return new ReceiverAtom<TIn, T>(_target);
        }
    }

    public static partial class Atom
    {
        public static ReceiverAtomBuilder<T> Receiver<T>(this Atom<T> target)
        {
            return new ReceiverAtomBuilder<T>(target);
        }
    }
}