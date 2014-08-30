using System;
using Dependable.Persistence;

namespace Dependable.Dispatcher
{
    public interface IWaitingForChildrenTransition
    {
        void Transit(Job job, Activity activity);
    }

    public class WaitingForChildrenTransition : IWaitingForChildrenTransition
    {
        readonly IPersistenceStore _persistenceStore;
        readonly IContinuationDispatcher _continuationDispatcher;
        readonly IActivityToContinuationConverter _activityToContinuationConverter;

        public WaitingForChildrenTransition(IPersistenceStore persistenceStore,
            IContinuationDispatcher continuationDispatcher,
            IActivityToContinuationConverter activityToContinuationConverter)
        {
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (continuationDispatcher == null) throw new ArgumentNullException("continuationDispatcher");

            _persistenceStore = persistenceStore;
            _continuationDispatcher = continuationDispatcher;
            _activityToContinuationConverter = activityToContinuationConverter;
        }

        public void Transit(Job job, Activity activity)
        {
            if (job == null) throw new ArgumentNullException("job");
            if (activity == null) throw new ArgumentNullException("activity");

            var converted = _activityToContinuationConverter.Convert(activity, job);

            _persistenceStore.Store(converted.Jobs);            
            
            job.Continuation = converted.Continuation;
            _continuationDispatcher.Dispatch(job, converted.Jobs);
        }        
    }
}