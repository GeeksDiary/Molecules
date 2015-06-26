using NSubstitute;
using Xunit;

namespace Molecules.Core.Tests
{
    public class SelectManyTests
    {
        readonly IApi _api = Substitute.For<IApi>();
        
        [Fact]
        public async void StandardQueryOperator()
        {
            _api.Call(1).Returns(2);
            _api.Call(2).Returns(3);
            _api.Call(3).Returns(4);

            var k = 
                from a in Atom.Of<int, int>(i => _api.Call(i))
                from b in Atom.Of(() => _api.Call(a))
                from c in Atom.Of(() => _api.Call(b))
                select a + b + c;

            var result = await k.Charge(1);

            Assert.Equal(9, result);
        }     
    }
}