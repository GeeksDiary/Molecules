using System;
using System.Collections.Generic;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dependable.Core.Tests
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
    }
}