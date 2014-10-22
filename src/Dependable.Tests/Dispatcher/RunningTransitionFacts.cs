using System;
using Dependable.Dispatcher;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class RunningTransitionFacts
    {
        readonly World _world = new World();

        [Fact]
        public void ShouldIncrementDispatchCountAndChangeToRunningStatus()
        {
            Job job = _world.NewJob.In(JobStatus.Ready);

            var mutated = _world.NewRunningTransition().Transit(job);

            var newDispatchCount = job.DispatchCount + 1;

            Assert.Equal(newDispatchCount, mutated.DispatchCount);
            _world.JobMutator.Mutations(job)
                .Verify(new Mutation {JobStatus = JobStatus.Running, DispatchCount = newDispatchCount});
        }
    }

    public static partial class WorldExtensions
    {
        public static RunningTransition NewRunningTransition(this World world)
        {
            if (world == null) throw new ArgumentNullException("world");

            return new RunningTransition(world.JobMutator);
        }
    }
}