using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class PipeTests
    {
        readonly IMethod _method = Substitute.For<IMethod>();
        
        [Fact]
        public async void ToUnaryFuncAtom()
        {
            _method.Call(1).Returns(2);
            _method.Call(2).Returns(3);

            Assert.Equal(3, 
                await Atom.Of((int i) => _method.Call(i))
                .Pipe(_method.Call)
                .Charge(1));            
        }
        
        [Fact]
        public async void ToFuncAtom()
        {
            _method.Call(1).Returns(1);
            _method.Nullary().Returns(2);

            Assert.Equal(2, 
                await Atom.Of((int i) => _method.Call(i))
                .Pipe(_method.Nullary)
                .Charge(1));

            Received.InOrder(() =>
            {
                _method.Call(1);
                _method.Nullary();
            });
        }

        [Fact]
        public async void ToActionAtom()
        {
            _method.Call(1).Returns(1);
            _method.Nullary().Returns(2);

            Assert.Equal(Unit.None,
                await Atom.Of((int i) => _method.Call(i))
                .Pipe(_method.Nullary)
                .Pipe(_method.Naked)
                .Charge(1));

            Received.InOrder(() =>
            {
                _method.Call(1);
                _method.Nullary();
                _method.Naked();
            });
        }        
    }
}