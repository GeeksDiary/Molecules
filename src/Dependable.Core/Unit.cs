using System.Threading.Tasks;

namespace Dependable.Core
{
    public struct Unit
    {
        public static Unit None => new Unit();

        public static Task<Unit> CompletedUnit => Task.FromResult(None);
    }
}