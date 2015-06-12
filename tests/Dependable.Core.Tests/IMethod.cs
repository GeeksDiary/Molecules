namespace Dependable.Core.Tests
{
    public interface IMethod
    {
        void Naked();

        int Nullary();

        void Void(int value);

        int Call(int value);
    }
}