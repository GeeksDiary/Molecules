using Dependable.Dispatcher;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace Dependable.Tests.Dispatcher
{
    public class EndTransitionFacts
    {
        public class SingleJob
        {
            readonly World _world = new World();

            [Theory]
            [InlineData(JobStatus.Completed, JobStatus.ReadyToComplete)]
            [InlineData(JobStatus.Poisoned, JobStatus.ReadyToPoison)]
            public void ShouldGoStraightIntoEndStatus(JobStatus status, JobStatus intermediaryStatus)
            {
                var job = _world.NewJob.In(JobStatus.Running);

                _world.NewEndTransition().Transit(job, status);

                _world.PrimitiveStatusChanger.DidNotReceive().Change<EndTransition>(job, intermediaryStatus);
                Assert.Equal(status, job.Status);                
            }       
        }

        public class InternalJobs
        {
            readonly World _world = new World();

            [Theory]
            [InlineData(JobStatus.Completed, JobStatus.ReadyToComplete)]
            [InlineData(JobStatus.Poisoned, JobStatus.ReadyToPoison)]
            public void ShouldGoToIntermediaryStatusBeforeEndStatus(JobStatus status,
                JobStatus intermediaryStatus)
            {
                var parent = _world.NewJob.In(JobStatus.WaitingForChildren);
                var child = _world.NewJob.In(JobStatus.Running).AsChildOf(parent);

                _world.NewEndTransition().Transit(child, status);

                Received.InOrder(() =>
                {
                    _world.PrimitiveStatusChanger.Change<EndTransition>(child, intermediaryStatus);
                    _world.PrimitiveStatusChanger.Change<EndTransition>(child, status);
                });
            }

            [Theory]
            [InlineData(JobStatus.Completed)]
            [InlineData(JobStatus.Poisoned)]
            public void ParentShouldGoToIntermediaryStatusBeforeEndStatus(JobStatus status)
            {
                var root = _world.NewJob.In(JobStatus.WaitingForChildren);
                var parent = _world.NewJob.In(JobStatus.WaitingForChildren).AsChildOf(root, JobStatus.Ready);
                var child = _world.NewJob.In(JobStatus.Running).AsChildOf(parent, JobStatus.Ready);

                _world.NewEndTransition().Transit(child, status);

                Received.InOrder(() =>
                {
                    _world.PrimitiveStatusChanger.Change<EndTransition>(parent, JobStatus.ReadyToComplete);
                    _world.PrimitiveStatusChanger.Change<EndTransition>(parent, JobStatus.Completed);
                });
            }

            [Theory]
            [InlineData(JobStatus.Completed)]
            [InlineData(JobStatus.Poisoned)]
            public void RootShouldNotGoToIntermediaryStatusBeforeEndStatus(JobStatus status)
            {
                var root = _world.NewJob.In(JobStatus.WaitingForChildren);
                var child = _world.NewJob.In(JobStatus.Running).AsChildOf(root, JobStatus.Ready);

                _world.NewEndTransition().Transit(child, status);

                _world.PrimitiveStatusChanger.DidNotReceive().Change<EndTransition>(root, JobStatus.ReadyToComplete);

            }
        }
        
        public class CompletingAnInternalJob
        {
            readonly World _world = new World();


            [Theory]
            [InlineData(JobStatus.Created)]
            [InlineData(JobStatus.Ready)]
            [InlineData(JobStatus.Running)]
            [InlineData(JobStatus.WaitingForChildren)]
            [InlineData(JobStatus.Failed)]
            [InlineData(JobStatus.ReadyToComplete)]
            [InlineData(JobStatus.ReadyToPoison)]
            public void DoesNotCompleteParentWithIncompleteSiblings(JobStatus siblingStatus)
            {
                var parent = _world.NewJob.In(JobStatus.WaitingForChildren);
                var sibling = _world.NewJob.In(siblingStatus).AsChildOf(parent, JobStatus.Ready);
                var siblingContinuation = ((Job)parent).Continuation.Find(sibling);

                _world.ContinuationDispatcher.Dispatch(parent).Returns(new[] { siblingContinuation });

                var completingChild = _world.NewJob.In(JobStatus.Running).AsChildOf(parent, JobStatus.Ready);

                _world.NewEndTransition().Transit(completingChild, JobStatus.Completed);

                Assert.Equal(JobStatus.WaitingForChildren, parent.Status);
            }

            [Fact]
            public void CompletesTheParent()
            {
                var parent = _world.NewJob.In(JobStatus.WaitingForChildren);
                var childA = _world.NewJob.In(JobStatus.Running).AsChildOf(parent);
                var childB = _world.NewJob.In(JobStatus.Running).AsChildOf(parent);

                var endTransition = _world.NewEndTransition();

                endTransition.Transit(childA, JobStatus.Completed);
                endTransition.Transit(childB, JobStatus.Completed);
                Assert.Equal(JobStatus.Completed, parent.Status);
            }

            [Fact]
            public void ShouldNotCompleteReadyToCompleteParentIfChildrenAreNotComplete()
            {
                var parent = _world.NewJob.In(JobStatus.ReadyToComplete);
                _world.NewJob.In(JobStatus.ReadyToComplete).AsChildOf(parent, JobStatus.Completed);

                _world.NewEndTransition().Transit(parent, JobStatus.Completed);

                Assert.Equal(JobStatus.ReadyToComplete, parent.Status);
            }

        }
    }

    public static partial class WorldExtensions
    {
        public static EndTransition NewEndTransition(this World world)
        {
            return new EndTransition(world.PersistenceStore, world.PrimitiveStatusChanger, world.ContinuationDispatcher);
        }
    }
}