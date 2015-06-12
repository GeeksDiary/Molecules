using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class AtomConnectivityTests
    {
        readonly IMethod _method = Substitute.For<IMethod>();
        
        [Fact]
        public async void PassThroughConnection()
        {
            _method.Call(1).Returns(2);
            _method.Call(2).Returns(3);

            Assert.Equal(3, await Atom.Of<int, int>(_method.Call).Connect(_method.Call).Charge(1));            
        }
        
        [Fact]
        public async void IgnoreIntermediaryConnection()
        {
            _method.Call(1).Returns(1);
            _method.Nullary().Returns(2);

            Assert.Equal(2, await Atom.Of<int, int>(_method.Call).Connect(_method.Nullary).Charge(1));

            Received.InOrder(() =>
            {
                _method.Call(1);
                _method.Nullary();
            });
        }

        [Fact]
        public async void IgnoreAllConnection()
        {
            _method.Call(1).Returns(1);
            _method.Nullary().Returns(2);

            Assert.Equal(Value.None,
                await Atom.Of<int, int>(_method.Call).Connect(_method.Nullary).Connect(_method.Naked).Charge(1));

            Received.InOrder(() =>
            {
                _method.Call(1);
                _method.Nullary();
                _method.Naked();
            });
        }
    }
}