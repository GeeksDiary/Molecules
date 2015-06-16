using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class AtomCreationTests
    {
        readonly IMethod _method = Substitute.For<IMethod>();

        [Fact]
        public async void UnaryFuncAtom()
        {
            _method.Call(1).Returns(2);
            Assert.Equal(2, await Atom.Of<int, int>(_method.Call).Charge(1));
        }

        [Fact]
        public async void NullaryFuncAtom()
        {
            _method.Nullary().Returns(1);
            Assert.Equal(1, await Atom.Of(_method.Nullary).Charge());
        }

        [Fact]
        public async void UnaryActionAtom()
        {
            Assert.Equal(Value.None, await Atom.Of<int>(_method.Void).Charge(1));
            _method.Received(1).Void(1);
        }

        [Fact]
        public async void ActionAtom()
        {
            Assert.Equal(Value.None, await Atom.Of(_method.Naked).Charge());
            _method.Received(1).Naked();
        }

        [Fact]
        public async void AnonMethod()
        {
            await Atom.Of(() => _method.Naked()).Charge();
            _method.Received(1).Naked();
        }
    }
}