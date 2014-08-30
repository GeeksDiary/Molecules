using System.Collections.Generic;
using System.Linq;
using Dependable.Dispatcher;
using NSubstitute;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class WaitingForChildrenTransitionFacts
    {
        readonly World _world = new World();

        [Fact]
        public void PersistsAllJobsReturnedByActivityToContinuationConverter()
        {
            IEnumerable<Job> newJobs = null;

            _world
                    .PersistenceStore
                    .When(s => s.Store(Arg.Any<IEnumerable<Job>>()))
                    .Do(c => newJobs = (IEnumerable<Job>)c.Args()[0]);

            var job = _world.NewJob.In(JobStatus.Running);
            var activity = Activity.Run<Test>(t => t.Run());
            var converted = new ConvertedActivity(new Continuation(), new[] {(Job) _world.NewJob});
            
            _world.ActivityToContinuationConverter.Convert(activity, job).Returns(converted);

            _world.NewWaitingForChildrenTransition().Transit(job, activity);

            Assert.Contains(converted.Jobs.Single(), newJobs);
        }

        [Fact]
        public void SetsConvertedContinuationToJobBeingAwaited()
        {
            var job = _world.NewJob.In(JobStatus.Running);
            var activity = Activity.Run<Test>(t => t.Run());
            var converted = new ConvertedActivity(new Continuation(), new[] { (Job)_world.NewJob });

            _world.ActivityToContinuationConverter.Convert(activity, job).Returns(converted);

            _world.NewWaitingForChildrenTransition().Transit(job, activity);

            Assert.Equal(converted.Continuation, ((Job) job).Continuation);
        }

        [Fact]
        public void InvokesContinuationDispatcher()
        {
            var job = _world.NewJob.In(JobStatus.Running);
            var activity = Activity.Run<Test>(t => t.Run());
            var converted = new ConvertedActivity(new Continuation(), new[] { (Job)_world.NewJob });

            _world.ActivityToContinuationConverter.Convert(activity, job).Returns(converted);

            _world.NewWaitingForChildrenTransition().Transit(job, activity);

            _world.ContinuationDispatcher.Received(1).Dispatch(job, converted.Jobs);
        }
    }

    public static partial class WorldExtensions
    {
        public static WaitingForChildrenTransition NewWaitingForChildrenTransition(this World world)
        {
            return
                new WaitingForChildrenTransition(
                    world.PersistenceStore,
                    world.ContinuationDispatcher,
                    world.ActivityToContinuationConverter);
        }
    }    
}