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
                await Atom.Func<int, int>(i => _signature.Func(i.Input))
                    .Then(i => _signature.Func(i.Input))
                    .AsReceivable()
                    .Of<int>()
                    .Charge(1));

            await Atom.Func<int, int>(i => _signature.Func(i.Input))
                .Then(i => _signature.Action(i.Input))
                .AsReceivable()
                .Of<int>()
                .Charge(1);

            _signature.Received(1).Action(2);
        }

        [Fact]
        public async void ShouldIgnoreFirstOutputIfNextDoesNotRequireInput()
        {
            _signature.Func(1).Returns(2);
            _signature.Func().Returns(3);

            Assert.Equal(3,
                await Atom.Func<int, int>(i => _signature.Func(i.Input))
                    .Then(() => _signature.Func())
                    .AsReceivable()
                    .Of<int>()
                    .Charge(1));
        }

        [Fact]
        public async void ShouldConnectMultipleActions()
        {
            await Atom.Action(() => _signature.Action())
                .Then(() => _signature.Action())
                .AsInvocable()
                .Charge();

            _signature.Received(2).Action();
        }
    }
}