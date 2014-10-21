using System;
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
        readonly Job _job;
        readonly SingleActivity _activity;
        readonly ConvertedActivity _converted;

        public WaitingForChildrenTransitionFacts()
        {
            _job = _world.NewJob.In(JobStatus.Running);
            _activity = Activity.Run<Test>(t => t.Run());
            _converted = new ConvertedActivity(new Continuation(), new[] {(Job) _world.NewJob});
        }

        [Fact]
        public void PersistsAllJobsCreatedForTheActivity()
        {
            IEnumerable<Job> newJobs = null;

            _world
                    .PersistenceStore
                    .When(s => s.Store(Arg.Any<IEnumerable<Job>>()))
                    .Do(c => newJobs = (IEnumerable<Job>)c.Args()[0]);
            
            _world.ActivityToContinuationConverter.Convert(_activity, _job).Returns(_converted);

            _world.NewWaitingForChildrenTransition().Transit(_job, _activity);

            Assert.Equal(_converted.Jobs.Single(), newJobs.Single());
        }

        [Fact]
        public void PersistsJobAsWaitingForChildrenWithContinuation()
        {            
            _world.ActivityToContinuationConverter.Convert(_activity, _job).Returns(_converted);

            _world.NewWaitingForChildrenTransition().Transit(_job, _activity);

            Assert.Equal(_converted.Continuation, _job.Continuation);

            _world.PrimitiveStatusChanger.Received(1)
                .Change<WaitingForChildrenTransition>(_job, JobStatus.WaitingForChildren);
        }

        [Fact]
        public void InvokesContinuationDispatcher()
        {
            _world.ActivityToContinuationConverter.Convert(_activity, _job).Returns(_converted);

            _world.NewWaitingForChildrenTransition().Transit(_job, _activity);

            _world.ContinuationDispatcher.Received(1).Dispatch(_job);
        }

        [Fact]
        public void RetriesIfContinuationDispatcherThrowsAnException()
        {
            var count = 0;

            _world.ActivityToContinuationConverter.Convert(_activity, _job).Returns(_converted);

            _world.ContinuationDispatcher.When(d => d.Dispatch(_job)).Do(c =>
            {
                if(count++ == 0)
                    throw new Exception("Doh");
            });
            
            _world.NewWaitingForChildrenTransition().Transit(_job, _activity);

            _world.ContinuationDispatcher.Received(2).Dispatch(_job);
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
                    world.ActivityToContinuationConverter,
                    world.RecoverableAction,
                    world.PrimitiveStatusChanger);
        }
    }    
}