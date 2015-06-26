using System.Threading.Tasks;

namespace Dependable.Core.Tests
{
    /// <summary>
    /// Representations of various signatures that can be 
    /// used to construct Atoms.
    /// </summary>
    public interface ISignature
    {
        int Func();

        int Func(int i);

        Task<int> AsyncFunc();

        Task<int> AsyncFunc(int i);

        void Action();

        void Action(int i);

        Task AsyncAction();

        Task AsyncAction(int i);
    }
}