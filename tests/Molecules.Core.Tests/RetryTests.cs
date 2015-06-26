using System;
using System.Collections.Generic;
using System.Diagnostics;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Molecules.Core.Tests
{
    public class RetryTests
    {
        readonly IApi _api = Substitute.For<IApi>();

        [Fact]
        public async void ExecutesNonFailingAtomOnlyOnce()
        {
            _api.Call(1).Returns(1);
            var a = Atom.Of<int, int>(i => _api.Call(i)).Retry();

            await a.Charge(1);

            _api.Received(1).Call(1);
        }

        [Fact]
        public async void FailsAfterReachingRetryCount()
        {
            _api.Call(1).Throws(new InvalidOperationException());

            var a = Atom.Of<int, int>(i => _api.Call(i)).Retry();

            await Assert.ThrowsAsync<InvalidOperationException>(() => a.Charge(1));
            _api.Received(2).Call(1);
        }

        [Fact]
        public async void RecoveringAtomsAreNotRetried()
        {
            var q = new Queue<Func<int>>();
            q.Enqueue(() => { throw new InvalidOperationException(); });
            q.Enqueue(() => 1);

            _api.Call(1).Returns(_ => q.Dequeue()());

            var a = Atom.Of<int, int>(i => _api.Call(i)).Retry(2);

            Assert.Equal(1, await a.Charge(1));
            _api.Received(2).Call(1);
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
                .Retry(1)
                .After(TimeSpan.FromSeconds(2))
                .Charge();

            Assert.True(watch.Elapsed >= TimeSpan.FromSeconds(2));
        }       
    }
}