using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class AtomCreationTests
    {
        readonly IMethod _method = Substitute.For<IMethod>();

        [Fact]
        public async void SimpleAtom()
        {
            _method.Call(1).Returns(2);
            Assert.Equal(2, await Atom.Of<int, int>(_method.Call).Charge(1));
        }

        [Fact]
        public async void NullaryAtom()
        {
            _method.Nullary().Returns(1);
            Assert.Equal(1, await Atom.Of(_method.Nullary).Charge());
        }

        [Fact]
        public async void VoidAtom()
        {
            Assert.Equal(Value.None, await Atom.Of<int>(_method.Void).Charge(1));
            _method.Received(1).Void(1);
        }

        [Fact]
        public async void NakedAtom()
        {
            Assert.Equal(Value.None, await Atom.Of(_method.Naked).Charge());
            _method.Received(1).Naked();
        }
    }

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