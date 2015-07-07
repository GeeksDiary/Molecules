using Xunit;

namespace Molecules.Core.Tests
{
    public class ConditionTests
    {
        [Fact]
        public async void ShouldChooseTheCorrectTurnAtTheJunction()
        {
            var f = Atom.Func<bool, bool>(b => b.Input)
                        .If(b => b, 
                            () => "a", 
                            () => "b")
                        .AsReceivable().Of<bool>();

            Assert.Equal("a", await f.Charge(true));
            Assert.Equal("b", await f.Charge(false));
        }
    }
}