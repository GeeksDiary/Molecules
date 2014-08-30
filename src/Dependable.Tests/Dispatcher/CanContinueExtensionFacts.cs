using Dependable.Dispatcher;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class CanContinueExtensionFacts
    {
        [Fact]
        public void ReturnsTrueForContinuableContinuationsWithoutAnyFailureHandlers()
        {
            var continuation = new Continuation {Status = JobStatus.Poisoned, ContinueAfterHandlingFailure = true};

            var result = continuation.CanContinue();

            Assert.True(result);
        }

        [Fact]
        public void ReturnsTrueIfAllFailureHandlersAreCompleted()
        {
            var continuation = new Continuation
            {
                Status = JobStatus.Poisoned,
                ContinueAfterHandlingFailure = true,
                OnAnyFailed = new Continuation {Status = JobStatus.Completed},
                OnAllFailed = new Continuation {Status = JobStatus.Completed}
            };

            var result = continuation.CanContinue();

            Assert.True(result);
        }

        [Fact]
        public void ReturnsFalseIfAnyFailureHandlerIsFailed()
        {
            var continuation = new Continuation
            {
                Status = JobStatus.Poisoned,
                ContinueAfterHandlingFailure = true,
                OnAllFailed = new Continuation {Status = JobStatus.Poisoned}
            };

            var result = continuation.CanContinue();

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueIfAllContinuationsInGroupAreCompleted()
        {
            var continuation = new Continuation
            {
                Status = JobStatus.Created,
                Children = new[] {new Continuation {Status = JobStatus.Completed}}
            };

            var result = continuation.CanContinue();

            Assert.True(result);
        }

        [Fact]
        public void ReturnsFalseIfAnyChildContinuationIsNotContinuable()
        {
            var continuation = new Continuation
            {
                Status = JobStatus.Created,
                Children = new[] {new Continuation {Status = JobStatus.Poisoned}}
            };

            var result = continuation.CanContinue();

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueForGroupWithFailedChildrenButSuccessfulFailureHandlers()
        {
            var continuation = new Continuation
            {
                Status = JobStatus.Created,
                Children = new[] {new Continuation {Status = JobStatus.Poisoned}},
                ContinueAfterHandlingFailure = true,
                OnAnyFailed = new Continuation {Status = JobStatus.Completed},
                OnAllFailed = new Continuation {Status = JobStatus.Completed}
            };

            var result = continuation.CanContinue();

            Assert.True(result);
        }

        [Fact]
        public void ReturnsFalseForGroupWithFailedChildrenAndFailedAnyFailureHandler()
        {
            var continuation = new Continuation
            {
                Status = JobStatus.Created,
                Children = new[] {new Continuation {Status = JobStatus.Poisoned}},
                ContinueAfterHandlingFailure = true,
                OnAnyFailed = new Continuation {Status = JobStatus.Poisoned},
                OnAllFailed = new Continuation {Status = JobStatus.Completed}
            };

            var result = continuation.CanContinue();

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalseForGroupWithFailedChildrenAndFailedAllFailureHandler()
        {
            var continuation = new Continuation
            {
                Status = JobStatus.Created,
                Children = new[] {new Continuation {Status = JobStatus.Poisoned}},
                ContinueAfterHandlingFailure = true,
                OnAnyFailed = new Continuation {Status = JobStatus.Completed},
                OnAllFailed = new Continuation {Status = JobStatus.Poisoned}
            };

            var result = continuation.CanContinue();

            Assert.False(result);
        }
    }
}