using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class AtomTests
    {
        public static int Double(int i)
        {
            return i*2;
        }

        [Fact]
        public async void ShouldInvokeTheImplementation()
        {
            Assert.Equal(1, await Atom.Of<string, int>(s => s.Length).Charge("a"));
        }

        [Fact]
        public async void ShouldConnectToAnotherAtom()
        {
            Assert.Equal(16, 
                await 
                Atom.Of<int, int>(i => i * 2)
                .Connect(Double)
                .Connect(Double)
                .Charge(2));
        }

        [Fact]
        public async void ShouldTakeTheCorrectTurnAtTheJunction()
        {
            var f = Atom.Of((bool i) => i).If(i => i, _ => "a", _ => "b");

            Assert.Equal("a", await f.Charge(true));
            Assert.Equal("b", await f.Charge(false));
        }

        [Fact]
        public async void ShouldInvokeAtomWithoutAnyInput()
        {
            var a = Atom.Of(() => "a");
            Assert.Equal("a", await a.Charge());
        }

        [Fact]
        public async void ShouldIgnoreIntermediateValueAndConnectToNoDomainAtom()
        {
            var m = Substitute.For<IMethod>();
            m.Call(0).ReturnsForAnyArgs(1);
            m.Nullary().ReturnsForAnyArgs(2);

            var a = Atom.Of<int, int>(m.Call).Connect(m.Nullary);

            Assert.Equal(2, await a.Charge(10));
            m.Received(1).Call(10);
            m.Received(1).Nullary();
        }
    }

    public interface IMethod
    {
        void Naked();

        int Nullary();

        void Void(int value);

        int Call(int value);
    }
}