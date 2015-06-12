using System.Threading.Tasks;

namespace Dependable.Core
{
    public struct Value
    {
        public static Value None => new Value();

        public static Task<Value> CompletedNone => Task.FromResult(None);
    }
}