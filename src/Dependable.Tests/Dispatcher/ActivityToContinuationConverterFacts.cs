using System.Linq;
using Dependable.Dispatcher;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class ActivityToContinuationConverterFacts
    {
        readonly World _world = new World();
        readonly JobManagementWrapper _parent;

        public ActivityToContinuationConverterFacts()
        {
            _parent = _world.NewJob.In(JobStatus.Running);
        }

        [Fact]
        public void ShouldCreateNewJobs()
        {
            var activity = Activity.Run<Test>(t => t.Run());
            
            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);
            
            Assert.Equal(1, converted.Jobs.Count());
        }

        [Fact]
        public void NewlyCreatedJobShouldHaveTheCorrectAttributes()
        {
            var parent = (Job) _parent;
            var activity = Activity.Run<Test>(t => t.RunWithArguments("a"));

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            var newJob = converted.Jobs.Single();

            Assert.Equal(typeof(Test), newJob.Type);
            Assert.Equal("a", (string)newJob.Arguments.Single());
            Assert.Equal(parent.Id, newJob.ParentId);
            Assert.Equal(parent.RootId, newJob.RootId);
        }

        [Fact]
        public void CreatesAJobForEachItemInGroup()
        {
            var activity =
                Activity.Parallel(
                    Activity.Run<Test>(t => t.Run()),
                    Activity.Run<Test>(t => t.Run()));

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            Assert.Equal(2, converted.Jobs.Count());
        }


        [Fact]
        public void ContinuationShouldPointToNewlyCreatedJob()
        {
            var activity = Activity.Run<Test>(t => t.Run());

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            Assert.Equal(converted.Jobs.Single().Id, converted.Continuation.Id);
        }

        [Fact]
        public void ConvertedGroupShouldContainAllActivitiesInTheGroup()
        {
            var activity =
                Activity.Parallel(
                    Activity.Run<Test>(t => t.Run()),
                    Activity.Run<Test>(t => t.Run()));


            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            Assert.Equal(2, converted.Continuation.Children.Count());
        }

        [Fact]
        public void CreatesJobsForNextActivityInCorrectOrder()
        {
            var activity = Activity
                .Run<Test>(t => t.RunWithArguments("a"))
                .Then<Test>(t => t.RunWithArguments("b"));

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            var first = converted.Jobs.Single(j => (string)j.Arguments.Single() == "a");
            var next = converted.Jobs.Single(j => (string)j.Arguments.Single() == "b");

            Assert.Equal(converted.Continuation.Id, first.Id);
            Assert.Equal(converted.Continuation.Next.Id, next.Id);
        }

        [Fact]
        public void CreatesJobsForOnAnyFailedActivity()
        {
            var activity = Activity.Run<Test>(t => t.Run()).WhenFailed(Activity.Run<Test>(t => t.Run()));

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            Assert.Equal(2, converted.Jobs.Count());
        }

        [Fact]
        public void ContinuationShouldPointToJobsCreatedForAnyFailedActivity()
        {
            var activity =
                Activity.Run<Test>(t => t.RunWithArguments("a"))
                    .WhenFailed(Activity.Run<Test>(t => t.RunWithArguments("b")));

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            var jobToRunOnAnyFailed = converted.Jobs.Single(j => (string)j.Arguments.Single() == "b");

            Assert.Equal(jobToRunOnAnyFailed.Id, converted.Continuation.OnAllFailed.Id);
        }

        [Fact]
        public void CreatesJobForOnAllFailedActivity()
        {
            var activity =
                Activity.Parallel(Activity.Run<Test>(t => t.Run())).WhenAllFailed(Activity.Run<Test>(t => t.Run()));

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            Assert.Equal(2, converted.Jobs.Count());
        }

        [Fact]
        public void ContinuationShouldPointToJobsCreatedForAllFailedActivity()
        {
            var activity =
                Activity.Parallel(Activity.Run<Test>(t => t.RunWithArguments("a")))
                    .WhenAllFailed(Activity.Run<Test>(t => t.RunWithArguments("b")));

            var converted = _world.NewActivityToContinuationConverter().Convert(activity, _parent);

            var jobToRunOnAllFailed = converted.Jobs.Single(j => (string)j.Arguments.Single() == "b");

            Assert.Equal(jobToRunOnAllFailed.Id, converted.Continuation.OnAllFailed.Id);
        }
    }

    public static partial class WorldExtensions
    {
        public static ActivityToContinuationConverter NewActivityToContinuationConverter(this World world)
        {
            return new ActivityToContinuationConverter(world.Now);
        }
    }
}