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
    }
}