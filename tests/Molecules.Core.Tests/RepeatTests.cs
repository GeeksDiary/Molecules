using Xunit;

namespace Molecules.Core.Tests
{
    public class RepeatTests
    {
        [Fact]
        public async void ShouldRepeatSpecifiedNumberOfTimes()
        {
            Assert.Equal(new[] {1, 1, 1}, 
                await Atom.Func<int, int>(i => i.Input).Repeat(3).Receiver().Listen<int>().Charge(1));
        }
    }
}