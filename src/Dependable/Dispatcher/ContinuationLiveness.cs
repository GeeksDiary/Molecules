using System;
using Dependable.Persistence;

namespace Dependable.Dispatcher
{
    public interface IContinuationLiveness
    {
        void Verify(Guid id);
    }

    /// <summary>
    /// Loads the specified job from the persistence store,
    /// and dispatches continuations. 
    /// Continuation dispatcher changes the status of each child job
    /// to Ready and dispatches it. In case this process fails before it's completed
    /// for all jobs in a continuation, we will have some jobs continuation in Ready state and some in
    /// Created state. This service is used to turn those jobs which are still in Created
    /// state to Ready and dispatch them.
    /// 
    /// This service loads the job from persistence store because, already dispatched
    /// child jobs may have changed the state of parent and we should not overwrite it.
    /// </summary>
    /// <remarks>
    /// Requires coordination.
    /// </remarks>
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

        public void Verify(Guid id)
        {
            // First load the current version of job
            var currentJob = _persistenceStore.Load(id);
            
            if (currentJob.Status != JobStatus.WaitingForChildren)
                return;

            _continuationDispatcher.Dispatch(currentJob);
        }
    }
}