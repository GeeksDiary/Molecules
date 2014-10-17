using System;
using Dependable.Dispatcher;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class FindFacts
    {
        readonly Dependable.Job _job = new Dependable.Job(Guid.NewGuid(), typeof (string), "Run", new object[0], Fixture.Now);

        [Fact]
        public void ShouldFindSingleChild()
        {
            var continuation = new Continuation {Id = _job.Id};

            var match = continuation.Find(_job);

            Assert.Equal(continuation, match);
        }

        [Fact]
        public void ShouldFindContinuationsInOnAnyFailure()
        {
            var continuation = new Continuation
            {
                Id = Guid.NewGuid(),
                OnAnyFailed = new Continuation {Id = _job.Id}
            };

            var match = continuation.Find(_job);

            Assert.Equal(continuation.OnAnyFailed, match);
        }

        [Fact]
        public void ShouldFindContinuationsInOnAllFailedHandler()
        {
            var continuation = new Continuation
            {
                Id = Guid.NewGuid(),
                OnAllFailed = new Continuation {Id = _job.Id}
            };

            var match = continuation.Find(_job);

            Assert.Equal(continuation.OnAllFailed, match);
        }

        [Fact]
        public void ShouldFindContinuationsInGroup()
        {
            var matchingChild = new Continuation {Id = _job.Id};

            var continuation = new Continuation
            {
                Children = new []
                {
                    new Continuation { Id = Guid.NewGuid() },
                    matchingChild
                }
            };

            var match = continuation.Find(_job);

            Assert.Equal(matchingChild, match);
        }

        [Fact]
        public void ShouldFindNextContinuation()
        {
            var continuation = new Continuation
            {
                Id = Guid.NewGuid(),
                Next = new Continuation {Id = _job.Id}
            };

            var match = continuation.Find(_job);

            Assert.Equal(continuation.Next, match);
        }

        [Fact]
        public void ShouldReturnNullIfNoMatchingContinuationIsFoundAnywhere()
        {
            var continuation = new Continuation
            {
                Children = new[] {new Continuation()},
                OnAnyFailed = new Continuation(),
                OnAllFailed = new Continuation()
            };

            var match = continuation.Find(_job);

            Assert.Null(match);
        }
    }
}