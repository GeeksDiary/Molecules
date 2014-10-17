using System;
using System.Collections.Generic;
using System.Linq;
using Dependable.Persistence;

namespace Dependable.Dispatcher
{
    public interface IContinuationDispatcher
    {
        Continuation[] Dispatch(Job job, IEnumerable<Job> children = null);
    }

    /// <summary>
    /// Used to evaluate to continuation of a given job and 
    /// dispatch the appropriate jobs.
    /// </summary>
    public class ContinuationDispatcher : IContinuationDispatcher
    {
        readonly IJobRouter _router;
        readonly IPrimitiveStatusChanger _primitiveStatusChanger;
        readonly IPersistenceStore _persistenceStore;

        public ContinuationDispatcher(IJobRouter router,
            IPrimitiveStatusChanger primitiveStatusChanger,
            IPersistenceStore persistenceStore)
        {
            if (router == null) throw new ArgumentNullException("router");
            if (primitiveStatusChanger == null) throw new ArgumentNullException("primitiveStatusChanger");
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");

            _router = router;
            _primitiveStatusChanger = primitiveStatusChanger;
            _persistenceStore = persistenceStore;
        }

        public Continuation[] Dispatch(Job job, IEnumerable<Job> children = null)
        {
            if (job == null) throw new ArgumentNullException("job");

            if (children == null)
                children = Enumerable.Empty<Job>();

            var readyContinuations = job.Continuation.PendingContinuations().ToArray();

            foreach (var continuation in readyContinuations)
                continuation.Status = JobStatus.Ready;

            _persistenceStore.Store(job);

            DispatchCore(readyContinuations, children);

            return readyContinuations;
        }

        /// <summary>
        /// Finds all schedulable jobs that are in 
        /// Created state. Then for each of them, 
        /// attempts to update the status to Ready.
        /// Once successful, routes the job.
        /// </summary>
        void DispatchCore(IEnumerable<Continuation> readyContinuations, IEnumerable<Job> children)
        {
            var childrenIndex = children.ToDictionary(c => c.Id, c => c);

            var schedulableJobs = (
                from @await in readyContinuations
                let j = childrenIndex.ContainsKey(@await.Id)
                    ? childrenIndex[@await.Id]
                    : _persistenceStore.Load(@await.Id)
                where j.Status == JobStatus.Created
                select j)
                .ToArray();

            foreach (var job in schedulableJobs)
            {
                _primitiveStatusChanger.Change<ContinuationDispatcher>(job, JobStatus.Ready);
                _router.Route(job);
            }
        }
    }
}