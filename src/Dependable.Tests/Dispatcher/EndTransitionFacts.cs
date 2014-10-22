using System;
using System.Collections.Generic;
using Dependable.Dispatcher;
using NSubstitute;
using NSubstitute.Core;
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
            [InlineData(JobStatus.Completed)]
            [InlineData(JobStatus.Poisoned)]
            public void ShouldGoStraightIntoEndStatus(JobStatus status)
            {
                var job = _world.NewJob.In(JobStatus.Running);

                _world.NewEndTransition().Transit(job, status);

                _world.JobMutator.Mutations(job).Verify(status);
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
                Job parent = _world.NewJob.In(JobStatus.WaitingForChildren);
                var child = _world.NewJob.In(JobStatus.Running).AsChildOf(ref parent);

                _world.NewEndTransition().Transit(child, status);

                _world.JobMutator.Mutations(child).Verify(intermediaryStatus, status);
            }

            [Theory]
            [InlineData(JobStatus.Completed)]
            [InlineData(JobStatus.Poisoned)]
            public void ParentShouldGoToIntermediaryStatusBeforeEndStatus(JobStatus status)
            {
                Job root = _world.NewJob.In(JobStatus.WaitingForChildren);
                Job parent = _world.NewJob.In(JobStatus.WaitingForChildren).AsChildOf(ref root, JobStatus.Ready);
                var child = _world.NewJob.In(JobStatus.Running).AsChildOf(ref parent, JobStatus.Ready);

                _world.NewEndTransition().Transit(child, status);

                _world.JobMutator.Mutations(parent).Verify(JobStatus.ReadyToComplete, JobStatus.Completed);
            }

            [Theory]
            [InlineData(JobStatus.Completed)]
            [InlineData(JobStatus.Poisoned)]
            public void RootShouldNotGoToIntermediaryStatusBeforeEndStatus(JobStatus status)
            {
                Job root = _world.NewJob.In(JobStatus.WaitingForChildren);
                var child = _world.NewJob.In(JobStatus.Running).AsChildOf(ref root, JobStatus.Ready);

                _world.NewEndTransition().Transit(child, status);

                _world.JobMutator.Mutations(root).Verify(JobStatus.Completed);
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
                Job parent = _world.NewJob.In(JobStatus.WaitingForChildren);
                var sibling = _world.NewJob.In(siblingStatus).AsChildOf(ref parent, JobStatus.Ready);
                var siblingContinuation = parent.Continuation.Find(sibling);

                _world.ContinuationDispatcher.Dispatch(parent).Returns(new[] { siblingContinuation });

                var completingChild = _world.NewJob.In(JobStatus.Running).AsChildOf(ref parent, JobStatus.Ready);

                _world.NewEndTransition().Transit(completingChild, JobStatus.Completed);

                Assert.Equal(JobStatus.WaitingForChildren, parent.Status);
            }

            [Fact]
            public void CompletesTheParent()
            {
                Job parent = _world.NewJob.In(JobStatus.WaitingForChildren);
                var childA = _world.NewJob.In(JobStatus.Running).AsChildOf(ref parent);
                var childB = _world.NewJob.In(JobStatus.Running).AsChildOf(ref parent);

                var endTransition = _world.NewEndTransition();

                endTransition.Transit(childA, JobStatus.Completed);
                endTransition.Transit(childB, JobStatus.Completed);

                parent = _world.PersistenceStore.Load(parent.Id);
                Assert.Equal(JobStatus.Completed, parent.Status);
            }
        }
    }

    public static partial class WorldExtensions
    {
        public static EndTransition NewEndTransition(this World world)
        {
            return new EndTransition(world.PersistenceStore, world.JobMutator, world.ContinuationDispatcher);
        }
    }
}