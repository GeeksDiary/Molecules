using System;
using System.Collections.Generic;
using System.Diagnostics;
using NSubstitute;
using Xunit;

namespace Molecules.Core.Tests
{
    public class CatchTests
    {
        readonly ISignature _signature = Substitute.For<ISignature>();

        [Fact]
        public async void ExecutesNonFailingAtomOnlyOnce()
        {
            await Atom.Of(() => _signature.Action()).Catch().AsInvocable().Charge();
            _signature.Received(1).Action();
        }

        [Fact]
        public async void FailsAfterReachingRetryCount()
        {
            _signature.When(s => s.Action()).Throw(new InvalidOperationException());
            var a = Atom.Of(() => _signature.Action()).Catch().AsInvocable();

            await Assert.ThrowsAsync<InvalidOperationException>(() => a.Charge());

            _signature.Received(2).Action();
        }

        [Fact]
        public async void RecoveringAtomsAreNotRetried()
        {
            var q = new Queue<Func<int>>();
            q.Enqueue(() => { throw new InvalidOperationException(); });
            q.Enqueue(() => 1);

            _signature.Func(1).Returns(_ => q.Dequeue()());

            var a = Atom.Of<int, int>(i => _signature.Func(i)).Catch().Retry(2).AsReceivable().Of<int>();

            Assert.Equal(1, await a.Charge(1));
            _signature.Received(2).Func(1);
        }

        [Fact]
        public async void ShouldWaitBeforeRetrying()
        {
            var q = new Queue<Action>();
            var watch = new Stopwatch();
            q.Enqueue(() =>
            {
                watch.Start();
                throw new InvalidOperationException();
            });

            q.Enqueue(() =>
            {
                watch.Stop();
            });

            await Atom.Of(() => q.Dequeue()())
                .Catch()
                .Wait(2)
                .Seconds
                .AsInvocable()                
                .Charge();

            Assert.True(watch.Elapsed >= TimeSpan.FromSeconds(2));
        }       
    }
}