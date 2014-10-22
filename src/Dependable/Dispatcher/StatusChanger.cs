using System;
using System.Collections.Generic;
using System.Linq;
using Dependable.Tracking;

namespace Dependable.Dispatcher
{
    public interface IStatusChanger
    {
        Job Change(Job job, JobStatus newStatus, Activity activity = null);
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

        static readonly JobStatus[] RunnableStatuses = {JobStatus.Ready, JobStatus.Failed, JobStatus.Running};

        static readonly JobStatus[] FallibleStatuses = {JobStatus.Ready, JobStatus.Running, JobStatus.Failed};

        static readonly JobStatus[] PoisonableStatus = {JobStatus.Running, JobStatus.ReadyToPoison, JobStatus.Poisoned};

        static readonly JobStatus[] AwaitableStatus = {JobStatus.Running, JobStatus.WaitingForChildren};

        readonly IRunningTransition _runningTransition;
        readonly IFailedTransition _failedTransition;
        readonly IEndTransition _endTransition;
        readonly IWaitingForChildrenTransition _waitingForChildrenTransition;
        readonly IJobMutator _jobMutator;

        public StatusChanger(IEventStream eventStream,
            IRunningTransition runningTransition,
            IFailedTransition failedTransition,
            IEndTransition endTransition,
            IWaitingForChildrenTransition waitingForChildrenTransition,
            IJobMutator jobMutator)
        {
            if (eventStream == null) throw new ArgumentNullException("eventStream");
            if (runningTransition == null) throw new ArgumentNullException("runningTransition");
            if (failedTransition == null) throw new ArgumentNullException("failedTransition");
            if (endTransition == null) throw new ArgumentNullException("endTransition");
            if (waitingForChildrenTransition == null) throw new ArgumentNullException("waitingForChildrenTransition");
            if (jobMutator == null) throw new ArgumentNullException("JobMutator");

            _runningTransition = runningTransition;
            _failedTransition = failedTransition;
            _endTransition = endTransition;
            _waitingForChildrenTransition = waitingForChildrenTransition;
            _jobMutator = jobMutator;
        }

        public Job Change(Job job, JobStatus newStatus, Activity activity = null)
        {
            switch (newStatus)
            {
                case JobStatus.Ready:
                    return CheckStatusAndInvoke(job, new[] {JobStatus.Created},
                        () => _jobMutator.Mutate<StatusChanger>(job, status: newStatus));
                case JobStatus.Running:
                    return CheckStatusAndInvoke(job, RunnableStatuses, () => _runningTransition.Transit(job));
                case JobStatus.Completed:
                    return CheckStatusAndInvoke(job, CompletableStatus,
                        () => _endTransition.Transit(job, JobStatus.Completed));
                case JobStatus.Failed:
                    return CheckStatusAndInvoke(job, FallibleStatuses, () => _failedTransition.Transit(job));
                case JobStatus.WaitingForChildren:
                    return CheckStatusAndInvoke(job, AwaitableStatus,
                        () => _waitingForChildrenTransition.Transit(job, activity));
                case JobStatus.Poisoned:
                    return CheckStatusAndInvoke(job, PoisonableStatus,
                        () => _endTransition.Transit(job, JobStatus.Poisoned));
            }

            return job;
        }

        static Job CheckStatusAndInvoke(Job job, IEnumerable<JobStatus> validStatus, Func<Job> action)
        {
            return validStatus.Contains(job.Status) ? action() : job;
        }
    }
}