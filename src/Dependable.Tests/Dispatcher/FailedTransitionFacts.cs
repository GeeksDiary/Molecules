using System;
using Dependable.Dispatcher;
using NSubstitute;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class FailedTransitionFacts
    {
        readonly World _world = new World();

        [Fact]
        public void ShouldUpdateRetryOnAndChangeToFailedStatus()
        {
            Job job = _world.NewJob.OfType<string>().In(JobStatus.Running);
            var configuration = new ActivityConfiguration(typeof (string)).WithRetryDelay(TimeSpan.FromSeconds(10));

            _world.Configuration.For(typeof (string)).Returns(configuration);

            _world.NewFailedTransition().Transit(job);

            _world.JobMutator.Mutations(job)
                .Verify(new Mutation
                {
                    JobStatus = JobStatus.Failed, 
                    RetryOn = _world.Now() + configuration.RetryDelay
                });
        }
    }

    public static partial class WorldExtensions
    {
        public static FailedTransition NewFailedTransition(this World world)
        {
            if (world == null) throw new ArgumentNullException("world");

            return new FailedTransition(world.Configuration, world.JobMutator, world.Now);
        }
    }
}