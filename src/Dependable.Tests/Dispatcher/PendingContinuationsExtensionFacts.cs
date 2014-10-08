using System.Linq;
using Dependable.Dispatcher;
using Xunit;
using Xunit.Extensions;

namespace Dependable.Tests.Dispatcher
{
    public class PendingContinuationsExtensionFacts
    {
        [Theory]
        [InlineData(JobStatus.Created)]
        [InlineData(JobStatus.Ready)]
        public void ReturnsSingleIncompleteContinuation(JobStatus incompleteStatus)
        {
            var continuation = new Continuation {Status = incompleteStatus};

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation, pending.Single());
        }

        [Fact]
        public void ReturnsOnFailedContinuationIfSingleContinuationIsFailed()
        {
            var continuation = new Continuation {Status = JobStatus.Poisoned, OnAllFailed = new Continuation()};

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.OnAllFailed, pending.Single());
        }

        [Fact]
        public void ShouldNotReturnNextIfSingleContinuationIsFailed()
        {
            var continuation = new Continuation {Status = JobStatus.Poisoned, Next = new Continuation()};

            var pending = continuation.PendingContinuations();

            Assert.Empty(pending);
        }

        [Fact]
        public void ReturnsNextIfSingleContinuationIsCompleted()
        {
            var continuation = new Continuation {Status = JobStatus.Completed, Next = new Continuation()};

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.Next, pending.Single());
        }

        [Fact]
        public void ShouldNotReturnNextIfSingleContinuationIsCreated()
        {
            var continuation = new Continuation {Next = new Continuation()};

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation, pending.Single());
        }

        [Fact]
        public void ReturnsAllIncompleteContinuationsInGrouped()
        {
            var created = new Continuation { Status = JobStatus.Created };
            var ready = new Continuation { Status = JobStatus.Ready };
            var completed = new Continuation {Status = JobStatus.Completed};
            var poisoned = new Continuation {Status = JobStatus.Poisoned};

            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[] {created, completed, poisoned, ready}
            };

            var pending = continuation.PendingContinuations().ToArray();

            Assert.Contains(created, pending);
            Assert.Contains(ready, pending);
            Assert.DoesNotContain(completed, pending);
            Assert.DoesNotContain(poisoned, pending);
        }

        [Fact]
        public void ReturnsNextIfAllContinuationsInGroupedIsComplete()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[] {new Continuation {Status = JobStatus.Completed}},
                Next = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.Next, pending.Single());
        }

        [Fact]
        public void ShouldNotReturnNextIfAnyContinuationInGroupIsFailed()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Completed},
                    new Continuation {Status = JobStatus.Poisoned}
                },
                Next = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Empty(pending);
        }

        [Fact]
        public void ShouldNotReturnNextIfAllChilrenAreNotComplete()
        {
            var continuation = new Continuation
            {
                Children = new[]
                {
                    new Continuation()
                },
                Next = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.DoesNotContain(continuation.Next, pending);
        }

        [Fact]
        public void ShouldReturnAnyFailedContinuationIfAnyContinuationInGroupIsFailed()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Poisoned}
                },
                OnAnyFailed = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.OnAnyFailed, pending.Single());
        }

        [Fact]
        public void ShouldReturnAllFailedContinuationIfAllContinuationsInGroupedIsFailed()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Poisoned}
                },
                OnAllFailed = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.OnAllFailed, pending.Single());
        }

        [Fact]
        public void ShouldNotReturnAllFailedContinuationIfAllContinuationsInGroupIsNotFailed()
        {
            var continuation = new Continuation
            {
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Completed},
                    new Continuation {Status = JobStatus.Poisoned}
                },
                OnAllFailed = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.DoesNotContain(continuation.OnAllFailed, pending);
        }

        [Fact]
        public void ShouldReturnAnyFailedContinuationIfAllContinuationsInGroupIsFailed()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Poisoned},
                    new Continuation {Status = JobStatus.Poisoned}
                },
                OnAnyFailed = new Continuation(),
                OnAllFailed = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Contains(continuation.OnAnyFailed, pending);
        }

        [Fact]
        public void ReturnsNextIfAnyFailedInRecoverableGroup()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Poisoned}
                },
                ContinueAfterHandlingFailure = true,
                Next = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.Next, pending.Single());
        }

        [Fact]
        public void ReturnsNextIfAnyFailedContinuationHasCompletedRecoverableContinuation()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Poisoned}
                },
                ContinueAfterHandlingFailure = true,
                OnAnyFailed = new Continuation {Status = JobStatus.Completed},
                Next = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.Next, pending.Single());
        }

        [Fact]
        public void ReturnsNextIfAllFailedHandlerHasCompletedRecoverableContinuation()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Parallel,
                Children = new[]
                {                    
                    new Continuation {Status = JobStatus.Poisoned}
                },
                ContinueAfterHandlingFailure = true,
                OnAllFailed = new Continuation {Status = JobStatus.Completed},
                Next = new Continuation()
            };

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.Next, pending.Single());
        }

        [Fact]
        public void ShouldReturnNextInSequence()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Sequence,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Completed},
                    new Continuation {Status = JobStatus.Created},
                    new Continuation {Status = JobStatus.Created}
                }
            };

            var pending = continuation.PendingContinuations();

            Assert.Equal(continuation.Children.ElementAt(1), pending.Single());
        }

        [Fact]
        public void ShouldNotReturnNextInSequenceIfCurrentIsFailed()
        {
            var continuation = new Continuation
            {
                Type = ContinuationType.Sequence,
                Children = new[]
                {
                    new Continuation {Status = JobStatus.Completed},
                    new Continuation {Status = JobStatus.Poisoned},
                    new Continuation {Status = JobStatus.Created}
                }
            };

            var pending = continuation.PendingContinuations();

            Assert.Empty(pending);
        }
    }
}