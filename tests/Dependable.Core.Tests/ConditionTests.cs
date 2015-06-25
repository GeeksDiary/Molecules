using Xunit;

namespace Dependable.Core.Tests
{
    public class ConditionTests
    {
        [Fact]
        public async void ShouldChooseTheCorrectTurnAtTheJunction()
        {
            var f = Atom.From<bool, bool>(b => b)
                        .If(b => b, 
                            () => "a", 
                            () => "b");

            Assert.Equal("a", await f.Charge(true));
            Assert.Equal("b", await f.Charge(false));
        }
    }
}