using System;
using System.Threading;
using System.Threading.Tasks;
using Dependable.Dispatcher;
using NSubstitute;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class JobCoordinatorFacts
    {
        readonly World _world = new World();

        public JobCoordinatorFacts()
        {
            _world.RecoverableAction.WhenForAnyArgs(r => r.Run(null)).Do(c => ((Action) c.Args()[0])());
        }

        [Fact]
        public void ShouldInvokeTheQueuedAction()
        {
            var job = _world.NewJob.In(JobStatus.Running);
            var invoked = false;

            _world.NewJobCoordinator().Run(job, () => invoked = true);

            Assert.True(invoked);
        }

        [Fact]
        public async Task ShouldSerializeRequestsForJobsInSameTree()
        {
            Job parent = _world.NewJob.In(JobStatus.WaitingForChildren);
            var childA = _world.NewJob.In(JobStatus.Running).AsChildOf(ref parent);
            var childB = _world.NewJob.In(JobStatus.Running).AsChildOf(ref parent);

            var childAStatusChange = new ManualResetEvent(false);
            var childAWaiting = new ManualResetEvent(false);

            var coordinator = _world.NewJobCoordinator();

            var childACompletion = Task.Run(() => coordinator.Run(childA, () =>
            {
                childAWaiting.Set();
                childAStatusChange.WaitOne();
            }));

            childAWaiting.WaitOne();

            var bExecuted = false;
            coordinator.Run(childB, () => bExecuted = true);

            Assert.False(bExecuted);

            childAStatusChange.Set();
            await childACompletion;

            Assert.True(bExecuted);
        }
    }

    public static partial class WorldExtensions
    {
        public static JobCoordinator NewJobCoordinator(this World world)
        {
            return new JobCoordinator(world.EventStream, world.RecoverableAction);
        }
    }
}