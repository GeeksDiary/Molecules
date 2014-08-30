using System;
using Dependable.Persistence;

namespace Dependable.Dispatcher
{
    public interface IContinuationLiveness
    {
        void Verify(Job job);
    }

    public class ContinuationLiveness : IContinuationLiveness
    {
        readonly IPersistenceStore _persistenceStore;
        readonly IContinuationDispatcher _continuationDispatcher;

        public ContinuationLiveness(IPersistenceStore persistenceStore, IContinuationDispatcher continuationDispatcher)
        {
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (continuationDispatcher == null) throw new ArgumentNullException("continuationDispatcher");

            _persistenceStore = persistenceStore;
            _continuationDispatcher = continuationDispatcher;
        }

        public void Verify(Job job)
        {
            // First load the current version of job
            var currentJob = _persistenceStore.Load(job.Id);
            
            if (currentJob.Status != JobStatus.WaitingForChildren)
                return;

            _continuationDispatcher.Dispatch(currentJob);
        }
    }
}