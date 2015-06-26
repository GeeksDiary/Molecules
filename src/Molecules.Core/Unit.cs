using System.Threading.Tasks;

namespace Molecules.Core
{
    public struct Unit
    {
        public static Unit Value => new Unit();

        public static Task<Unit> CompletedTask => Task.FromResult(Value);
    }
}