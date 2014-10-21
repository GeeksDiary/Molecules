using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dependable.Dispatcher;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace Dependable.Tests.Dispatcher
{
    public class JobQueueFactoryFacts
    {
        readonly World _world = new World();

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
        public async Task ShouldOnlyRecoverJobsInRecoverableState(JobStatus status, bool recoverable)
        {
            Dependable.Job job = _world.NewJob.In(status);
            Dependable.Job next = _world.NewJob;

            _world.PersistenceStore.LoadBy(status).Returns(new[] {job});

            var configuration = _world.NewJobQueueFactory().Create();

            configuration.Default.Write(next);
            var first = await configuration.Default.Read();

            Assert.Equal(recoverable ? job : next, first);
        }

        [Fact]
        public async Task EachActivitySpecificQueueIsCreatedWithOnlyMatchingJobs()
        {
            var readyString = _world.NewJob.OfType<string>().In(JobStatus.Ready);
            var readyShort = _world.NewJob.OfType<short>().In(JobStatus.Ready);
            var readyInt = _world.NewJob.OfType<int>().In(JobStatus.Ready);

            _world.PersistenceStore.LoadBy(JobStatus.Ready)
                .Returns(new Dependable.Job[] {readyString, readyShort, readyInt});

            var configuration = _world.NewJobQueueFactory(activityConfiguration: new[]
            {
                new ActivityConfiguration(typeof (string)),
                new ActivityConfiguration(typeof (short))
            })
                .Create();

            var stringQueue = configuration.ActivitySpecificQueues[typeof (string)];
            var shortQueue = configuration.ActivitySpecificQueues[typeof (short)];
            var defaultQueue = configuration.Default;

            var nextString = _world.NewJob.OfType<string>();
            var nextShort = _world.NewJob.OfType<short>();
            var nextInt = _world.NewJob.OfType<int>();

            stringQueue.Write(nextString);
            shortQueue.Write(nextShort);
            defaultQueue.Write(nextInt);

            Assert.Equal(readyString, await stringQueue.Read());
            Assert.Equal(readyShort, await shortQueue.Read());
            Assert.Equal(readyInt, await configuration.Default.Read());

            Assert.Equal(nextString, await stringQueue.Read());
            Assert.Equal(nextShort, await shortQueue.Read());
            Assert.Equal(nextInt, await defaultQueue.Read());
        }

        [Fact]
        public async Task ShouldProvideCorrectSuspendedCountForSpecificQueue()
        {
            var suspendedJob = _world.NewJob.OfType<string>().In(JobStatus.Ready);
            Dependable.Job job = _world.NewJob.OfType<string>().In(JobStatus.Ready);

            _world.PersistenceStore.LoadSuspended(typeof (string), 1).Returns(new Dependable.Job[] {suspendedJob});
            _world.PersistenceStore.CountSuspended(typeof (string)).Returns(1);

            var configuration = _world.NewJobQueueFactory(activityConfiguration: new[]
            {
                new ActivityConfiguration(typeof (string)).WithMaxQueueLength(1)
            }).Create();

            var stringQueue = configuration.ActivitySpecificQueues[typeof (string)];

            Assert.Equal(suspendedJob, await stringQueue.Read());

            stringQueue.Write(job);
            Assert.Equal(job, await stringQueue.Read());
        }

        [Fact]
        public async Task ShouldProvideCorrectSuspendedCountForDefaultQueue()
        {
            Dependable.Job suspendedIntJob = _world.NewJob.OfType<int>().In(JobStatus.Ready);
            Dependable.Job intJob = _world.NewJob.OfType<int>().In(JobStatus.Ready);

            _world.PersistenceStore.LoadSuspended(Arg.Any<IEnumerable<Type>>(), 1).Returns(new[] {suspendedIntJob});
            _world.PersistenceStore.CountSuspended(typeof (string)).Returns(1);
            _world.PersistenceStore.CountSuspended(null).Returns(2);

            var configuration = _world.NewJobQueueFactory(
                activityConfiguration: new[]
                {
                    new ActivityConfiguration(typeof (string)).WithMaxQueueLength(1)
                },
                defaultActivityConfiguration: new ActivityConfiguration().WithMaxQueueLength(1)).Create();

            var defaultQueue = configuration.Default;
            Assert.Equal(suspendedIntJob, await defaultQueue.Read());

            defaultQueue.Write(intJob);
            Assert.Equal(intJob, await defaultQueue.Read());
        }
    }

    public static partial class WorldExtensions
    {
        public static JobQueueFactory NewJobQueueFactory(this World world,
            IEnumerable<ActivityConfiguration> activityConfiguration = null,
            ActivityConfiguration defaultActivityConfiguration = null)
        {
            var configuration = Substitute.For<IDependableConfiguration>();

            configuration.DefaultActivityConfiguration.Returns(
                defaultActivityConfiguration ?? new ActivityConfiguration());

            configuration.ActivityConfiguration.Returns(
                activityConfiguration ?? Enumerable.Empty<ActivityConfiguration>());

            return new JobQueueFactory(world.PersistenceStore, configuration, world.EventStream, world.RecoverableAction);
        }
    }
}