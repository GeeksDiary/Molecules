using Xunit;

namespace Molecules.Core.Tests
{
    public class RepeatTests
    {
        [Fact]
        public async void ShouldRepeatSpecifiedNumberOfTimes()
        {
            Assert.Equal(new[] {1, 1, 1}, 
                await Atom.Of((int i) => i).Repeat(3).Charge(1));
        }
    }
}