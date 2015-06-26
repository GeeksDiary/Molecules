using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class RepeatTests
    {
        readonly IApi _api = Substitute.For<IApi>();

        [Fact]
        public async void ShouldRepeatSpecifiedNumberOfTimes()
        {
            _api.Call(1).Returns(1);
            var a = Atom.Of((int i) => _api.Call(i)).Repeat(3);

            var result = await a.Charge(1);

            Assert.Equal(new[] {1, 1, 1}, result);
        } 
    }
}