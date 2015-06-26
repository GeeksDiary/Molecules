using Xunit;

namespace Molecules.Core.Tests
{
    public class RepeatTests
    {
        [Fact]
        public async void ShouldRepeatSpecifiedNumberOfTimes()
        {
            var a = Atom.Of((int i) => i).Repeat(3);

            var result = await a.Charge(1);

            Assert.Equal(new[] {1, 1, 1}, result);
        } 
    }
}