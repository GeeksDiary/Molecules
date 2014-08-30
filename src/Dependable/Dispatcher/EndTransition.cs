using System;
using System.Collections.Generic;
using System.Linq;
using Dependable.Persistence;

namespace Dependable.Dispatcher
{
    public interface IEndTransition
    {
        void Transit(Job job, JobStatus endStatus);
    }

    public class EndTransition : IEndTransition
    {
        readonly IPersistenceStore _jobRepository;
        readonly IPrimitiveStatusChanger _primitiveStatusChanger;
        readonly IContinuationDispatcher _continuationDispatcher;

        static readonly Dictionary<JobStatus, JobStatus> PendingEndStatus =
            new Dictionary<JobStatus, JobStatus>
            {
                {JobStatus.Completed, JobStatus.ReadyToComplete},
                {JobStatus.Poisoned, JobStatus.ReadyToPoison}
            };

        static readonly IEnumerable<JobStatus> EndStatuses = new[]
        {JobStatus.Completed, JobStatus.Poisoned};

        public EndTransition(IPersistenceStore jobRepository,
            IPrimitiveStatusChanger primitiveStatusChanger,
            IContinuationDispatcher continuationDispatcher)
        {
            if (jobRepository == null) throw new ArgumentNullException("jobRepository");
            if (primitiveStatusChanger == null) throw new ArgumentNullException("primitiveStatusChanger");
            if (continuationDispatcher == null) throw new ArgumentNullException("continuationDispatcher");

            _jobRepository = jobRepository;
            _primitiveStatusChanger = primitiveStatusChanger;
            _continuationDispatcher = continuationDispatcher;
        }

        public void Transit(Job job, JobStatus endStatus)
        {
            if (job == null) throw new ArgumentNullException("job");

            if (!EndStatuses.Contains(endStatus))
                throw new ArgumentOutOfRangeException("endStatus", endStatus, "Not a valid end status.");

            if (job.ParentId == null)
                _primitiveStatusChanger.Change<EndTransition>(job, endStatus);
            else
                EndTree(job, endStatus);
        }

        void EndTree(Job job, JobStatus endStatus)
        {
            if (job.ParentId == null)
            {
                _primitiveStatusChanger.Change<EndTransition>(job, JobStatus.Completed);
                return;
            }

            _primitiveStatusChanger.Change<EndTransition>(job, PendingEndStatus[endStatus]);

            /*
             * First, load the parent, find the await record for this job and 
             * update its status to end status.
             */
            var parent = _jobRepository.Load(job.ParentId.Value);
            var @await = parent.Continuation.Find(job);
            @await.Status = endStatus;

            /*
             * After we set the await's status, invoke ContinuationDispatcher to 
             * ensure any pending await for parent is dispatched.
             * If ContinuationDispatcher returns any awaits, that means parent job is not
             * ready for completion. We simply save the status change we made
             * and change current job's status to desired end status.
             * Otherwise, we complete the parent and then change current job's 
             * status to desired end status.
             */
            var pendingAwaits = _continuationDispatcher.Dispatch(parent);

            if (pendingAwaits.Any())
                _jobRepository.Store(parent);
            else
                EndTree(parent, JobStatus.Completed);

            _primitiveStatusChanger.Change<EndTransition>(job, endStatus);
        }
    }
}