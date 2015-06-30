using NSubstitute;
using Xunit;

namespace Molecules.Core.Tests
{
    public class WhileTests
    {
        readonly ISignature _signature = Substitute.For<ISignature>();

        [Fact]
        public async void ShouldRepeatBodyUntilTestPasess()
        {
            _signature.Func(0).ReturnsForAnyArgs(c => c.Args()[0]);
            var a = Atom.Of((int k) => k)
                .While(k => k < 10)
                .Do(i => _signature.Func(i))
                .With((k, _) => k + 1);

            Assert.Equal(9, await a.Charge(0));
            _signature.ReceivedWithAnyArgs(10).Func(0);
        }

        [Fact]
        public async void ShouldNotInvokeTheBodyIfTestPassesDuringFirstAttempt()
        {
            _signature.Func(0).Returns(0);
            var a = Atom.Of((int k) => k)
                .While(k => k != 0)
                .Do(i => _signature.Func(i))
                .With((k, _) => k + 1);

            await a.Charge(0);

            _signature.DidNotReceiveWithAnyArgs().Func(0);
        }
    }
}