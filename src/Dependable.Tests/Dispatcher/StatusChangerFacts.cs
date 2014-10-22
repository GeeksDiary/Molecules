using System;
using Dependable.Dispatcher;
using NSubstitute;
using Xunit.Extensions;

namespace Dependable.Tests.Dispatcher
{
    public class StatusChangerFacts
    {
        readonly World _world = new World();
        readonly Func<StatusChanger> _changeStatus;

        public StatusChangerFacts()
        {
            _changeStatus =
                () =>
                    new StatusChanger(_world.EventStream, _world.RunningTransition,
                        _world.FailedTransition, _world.EndTransition, _world.WaitingForChildrenTransition,
                        _world.JobMutator);
        }

        [Theory]
        [InlineData(JobStatus.Ready)]
        [InlineData(JobStatus.Running)]
        [InlineData(JobStatus.Failed)]
        public void TransitsToRunningFrom(JobStatus @from)
        {
            var job = _world.NewJob.In(@from);
            _changeStatus().Change(job, JobStatus.Running);
            _world.RunningTransition.Received(1).Transit(job);
        }

        [Theory]
        [InlineData(JobStatus.Created)]
        [InlineData(JobStatus.WaitingForChildren)]
        [InlineData(JobStatus.ReadyToComplete)]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.ReadyToPoison)]
        [InlineData(JobStatus.Poisoned)]
        public void DoesNotTransitToRunningFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.Running);
            _world.RunningTransition.DidNotReceive().Transit(job);
        }

        [Theory]
        [InlineData(JobStatus.Running)]
        [InlineData(JobStatus.WaitingForChildren)]
        [InlineData(JobStatus.ReadyToComplete)]
        public void TransitsToCompletedFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.Completed);
            _world.EndTransition.Received(1).Transit(job, JobStatus.Completed);
        }

        [Theory]
        [InlineData(JobStatus.Created)]
        [InlineData(JobStatus.Ready)]
        [InlineData(JobStatus.Failed)]
        [InlineData(JobStatus.ReadyToPoison)]
        [InlineData(JobStatus.Poisoned)]
        public void DoesNotTransitToCompletedFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.Completed);
            _world.EndTransition.DidNotReceive().Transit(job, JobStatus.Completed);
        }

        [Theory]
        [InlineData(JobStatus.Ready)]
        [InlineData(JobStatus.Running)]
        [InlineData(JobStatus.Failed)]
        public void TransitsToFailedFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.Failed);
            _world.FailedTransition.Received(1).Transit(job);
        }

        [Theory]
        [InlineData(JobStatus.Created)]        
        [InlineData(JobStatus.WaitingForChildren)]        
        [InlineData(JobStatus.ReadyToComplete)]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.ReadyToPoison)]
        [InlineData(JobStatus.Poisoned)]
        public void DoesNotTransitToFailedFrom(JobStatus status)
        {
            Job job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.Failed);
            _world.FailedTransition.DidNotReceive().Transit(job);
        }

        [Theory]
        [InlineData(JobStatus.Running)]
        [InlineData(JobStatus.ReadyToPoison)]
        public void TransitsToPoisonedFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.Poisoned);
            _world.EndTransition.Received(1).Transit(job, JobStatus.Poisoned);
        }

        [Theory]
        [InlineData(JobStatus.Created)]
        [InlineData(JobStatus.Ready)]
        [InlineData(JobStatus.WaitingForChildren)]
        [InlineData(JobStatus.Failed)]
        [InlineData(JobStatus.ReadyToComplete)]
        [InlineData(JobStatus.Completed)]        
        public void DoesNotTransitToPoisonedFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.Poisoned);
            _world.EndTransition.DidNotReceive().Transit(job, JobStatus.Poisoned);
        }
        
        [Theory]
        [InlineData(JobStatus.Running)]
        [InlineData(JobStatus.WaitingForChildren)]
        public void TransitsToWaitingForChildrenFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.WaitingForChildren);
            _world.WaitingForChildrenTransition.Received(1).Transit(job, null);
        }

        [Theory]
        [InlineData(JobStatus.Created)]
        [InlineData(JobStatus.Ready)]
        [InlineData(JobStatus.Failed)]
        [InlineData(JobStatus.ReadyToComplete)]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.ReadyToPoison)]
        [InlineData(JobStatus.Poisoned)]
        public void DoesNotTransitToWaitingForChildrenFrom(JobStatus status)
        {
            var job = _world.NewJob.In(status);
            _changeStatus().Change(job, JobStatus.WaitingForChildren);
            _world.WaitingForChildrenTransition.DidNotReceive().Transit(job, null);
        }
    }
}