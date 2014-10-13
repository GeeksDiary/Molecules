using System;
using System.Collections.Generic;
using System.Linq;
using Dependable.Tracking;

namespace Dependable.Dispatcher
{
    public interface IStatusChanger
    {
        void Change(Job job, JobStatus newStatus, Activity activity = null);
    }

    /// <summary>
    /// Enforces the state transition rules and invokes the appropriate 
    /// XxxTransition implementation.
    /// 
    /// All state transitions are performed through this component and 
    /// therefore considered as the single source of truth for 
    /// state machine.
    /// </summary>
    public class StatusChanger : IStatusChanger
    {
        static readonly JobStatus[] CompletableStatus =
        {
            JobStatus.Running, 
            JobStatus.WaitingForChildren, 
            JobStatus.ReadyToComplete,
            JobStatus.Completed
        };

        static readonly JobStatus[] RunnableStatus = {JobStatus.Ready, JobStatus.Failed, JobStatus.Running};

        static readonly JobStatus[] FailableStatus = {JobStatus.Running, JobStatus.Failed};

        static readonly JobStatus[] PoisonableStatus = {JobStatus.Running, JobStatus.ReadyToPoison, JobStatus.Poisoned};

        static readonly JobStatus[] AwaitableStatus = {JobStatus.Running, JobStatus.WaitingForChildren};

        readonly IEventStream _eventStream;
        readonly IRunningTransition _runningTransition;
        readonly IFailedTransition _failedTransition;
        readonly IEndTransition _endTransition;
        readonly IWaitingForChildrenTransition _waitingForChildrenTransition;
        readonly IPrimitiveStatusChanger _primitiveStatusChanger;

        public StatusChanger(IEventStream eventStream,
            IRunningTransition runningTransition,
            IFailedTransition failedTransition,
            IEndTransition endTransition,
            IWaitingForChildrenTransition waitingForChildrenTransition,
            IPrimitiveStatusChanger primitiveStatusChanger)
        {
            if (eventStream == null) throw new ArgumentNullException("eventStream");
            if (runningTransition == null) throw new ArgumentNullException("runningTransition");
            if (failedTransition == null) throw new ArgumentNullException("failedTransition");
            if (endTransition == null) throw new ArgumentNullException("endTransition");
            if (waitingForChildrenTransition == null) throw new ArgumentNullException("waitingForChildrenTransition");
            if (primitiveStatusChanger == null) throw new ArgumentNullException("primitiveStatusChanger");

            _eventStream = eventStream;
            _runningTransition = runningTransition;
            _failedTransition = failedTransition;
            _endTransition = endTransition;
            _waitingForChildrenTransition = waitingForChildrenTransition;
            _primitiveStatusChanger = primitiveStatusChanger;
        }

        public void Change(Job job, JobStatus newStatus, Activity activity = null)
        {
            var originalStatus = job.Status;
            switch (newStatus)
            {
                case JobStatus.Ready:
                    CheckStatusAndInvoke(job, new[] {JobStatus.Created}, () => _primitiveStatusChanger.Change<StatusChanger>(job, newStatus));
                    break;
                case JobStatus.Running:
                    CheckStatusAndInvoke(job, RunnableStatus, () => _runningTransition.Transit(job));
                    break;
                case JobStatus.Completed:
                    CheckStatusAndInvoke(job, CompletableStatus,
                        () => _endTransition.Transit(job, JobStatus.Completed));
                    break;
                case JobStatus.Failed:
                    CheckStatusAndInvoke(job, FailableStatus, () => _failedTransition.Transit(job));
                    break;
                case JobStatus.WaitingForChildren:
                    CheckStatusAndInvoke(job, AwaitableStatus,
                        () => _waitingForChildrenTransition.Transit(job, activity));
                    break;
                case JobStatus.Poisoned:
                    CheckStatusAndInvoke(job, PoisonableStatus,
                        () => _endTransition.Transit(job, JobStatus.Poisoned));
                    break;                
            }

            if (job.Status == originalStatus)
            {
                _eventStream.Publish<StatusChanger>(
                    EventType.JobStatusChangeRejected,                    
                    EventProperty.JobSnapshot(job),
                    EventProperty.FromStatus(originalStatus),
                    EventProperty.ToStatus(newStatus));
            }
        }

        static void CheckStatusAndInvoke(Job job, IEnumerable<JobStatus> validStatus, Action action)
        {
            if (validStatus.Contains(job.Status))
                action();
        }
    }
}