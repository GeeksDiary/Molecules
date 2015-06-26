using NSubstitute;
using Xunit;

namespace Molecules.Core.Tests
{
    public class ThenTests
    {
        readonly ISignature _signature = Substitute.For<ISignature>();

        [Fact]
        public async void ShouldPipeFirstAtomsOutputToNext()
        {
            _signature.Func(1).Returns(2);
            _signature.Func(2).Returns(3);

            Assert.Equal(3,
                await Atom.Of((int i) => _signature.Func(i))
                    .Then(i => _signature.Func(i))
                    .Charge(1));

            await Atom.Of((int i) => _signature.Func(i))
                .Then(i => _signature.Action(i))
                .Charge(1);

            _signature.Received(1).Action(2);
        }

        [Fact]
        public async void ShouldIgnoreFirstOutputIfNextDoesNotRequireInput()
        {
            _signature.Func(1).Returns(2);
            _signature.Func().Returns(3);

            Assert.Equal(3,
                await Atom.Of((int i) => _signature.Func(i))
                    .Then(() => _signature.Func())
                    .Charge(1));
        }

        [Fact]
        public async void ShouldConnectMultipleActions()
        {
            await Atom.Of(() => _signature.Action())
                .Then(() => _signature.Action())
                .Charge();

            _signature.Received(2).Action();
        }
    }
}