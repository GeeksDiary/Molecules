using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class ThenTests
    {
        readonly IMethod _method = Substitute.For<IMethod>();
        
        [Fact]
        public async void StandardQueryOperator()
        {
            _method.Call(1).Returns(2);
            _method.Call(2).Returns(3);
            _method.Call(3).Returns(4);

            var k = 
                from a in Atom.From<int, int>(i => _method.Call(i))
                from b in Atom.From(() => _method.Call(a))
                from c in Atom.From(() => _method.Call(b))
                select a + b + c;

            var result = await k.Charge(1);

            Assert.Equal(9, result);
        }     
    }
}