using System;
using System.Collections.Generic;
using System.Linq;
using Dependable.Dispatcher;
using Dependable.Recovery;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace Dependable.Tests.Dispatcher
{
    public class JobQueueRecoveryFacts
    {
        readonly World _world = new World();
        readonly IJobQueue _defaultQueue = Substitute.For<IJobQueue>();

        public JobQueueRecoveryFacts()
        {
            _defaultQueue.Initialize(null).ReturnsForAnyArgs(new Job[0]);
        }

        [Theory]
        [InlineData(JobStatus.Created, false)]
        [InlineData(JobStatus.Ready, true)]
        [InlineData(JobStatus.Running, true)]
        [InlineData(JobStatus.WaitingForChildren, true)]
        [InlineData(JobStatus.Failed, true)]
        [InlineData(JobStatus.ReadyToPoison, true)]
        [InlineData(JobStatus.Poisoned, false)]
        [InlineData(JobStatus.ReadyToComplete, true)]
        [InlineData(JobStatus.Completed, false)]
        public void ShouldOnlyRecoverJobsInRecoverableState(JobStatus status, bool recoverable)
        {
            Job job = _world.NewJob.In(status);
            
            _world.PersistenceStore.LoadBy(status).Returns(new[] {job});
            
            _world.NewJobQueueRecovery(_defaultQueue).Recover();

            _defaultQueue.Received(1)
                .Initialize(recoverable
                    ? Arg.Is<Job[]>(a => a.Contains(job))
                    : Arg.Is<Job[]>(a => a.Length == 0));
        }

        [Fact]
        public void NextSpecificQueueInListGetsOnlyRemainingItems()
        {
            var rest = new Job[] {_world.NewJob.In(JobStatus.Ready)};
            
            var qa = Substitute.For<IJobQueue>();
            qa.Initialize(null).ReturnsForAnyArgs(rest);

            var qb = Substitute.For<IJobQueue>();

            var specificQueues = new Dictionary<Type, IJobQueue>
            {
                {typeof(string), qa},
                {typeof(int), qb}
            };

            _world.Router.SpecificQueues.Returns(specificQueues);
            
            _world.NewJobQueueRecovery(_defaultQueue).Recover();

            qb.Received(1).Initialize(rest);
        }

        [Fact]
        public void DefaultQueueGetsItemsNotConsumedBySpecificQueues()
        {
            var rest = new Job[] { _world.NewJob.In(JobStatus.Ready) };

            var qa = Substitute.For<IJobQueue>();
            qa.Initialize(null).ReturnsForAnyArgs(rest);

            var specificQueues = new Dictionary<Type, IJobQueue>
            {
                {typeof(string), qa},
            };

            _world.Router.SpecificQueues.Returns(specificQueues);

            _world.NewJobQueueRecovery(_defaultQueue).Recover();

            _defaultQueue.Received(1).Initialize(rest);
        }
    }

    public static partial class WorldExtensions
    {
        public static JobQueueRecovery NewJobQueueRecovery(this World world, IJobQueue defaultQueue)
        {
            world.Router.DefaultQueue.Returns(defaultQueue);
            return new JobQueueRecovery(world.PersistenceStore, world.Router);
        }
    }
}