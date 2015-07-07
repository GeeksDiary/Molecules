using System.Collections.Generic;
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
            var q = new Queue<int>(new[] {0, 1, 2});

            var a = Atom.Func(() => q.Dequeue())
                .While(k => k < 2)
                .Do(i => _signature.Func(i.Input))
                .AsReceivable()
                .Of<int>();

            Assert.Equal(1, await a.Charge(0));
            _signature.ReceivedWithAnyArgs(2).Func(0);
        }

        [Fact]
        public async void ShouldNotInvokeTheBodyIfTestPassesDuringFirstAttempt()
        {
            _signature.Func(0).Returns(0);
            var a =
                Atom.Func<int, int>(k => k.Input)
                    .While(k => k != 0)
                    .Do(i => _signature.Func(i.Input))
                    .AsReceivable()
                    .Of<int>();

            await a.Charge(0);

            _signature.DidNotReceiveWithAnyArgs().Func(0);
        }
    }
}