using System;
using Dependable.Persistence;
using Dependable.Recovery;

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
        readonly IRecoverableAction _recoverableAction;
        readonly IPrimitiveStatusChanger _primitiveStatusChanger;

        public WaitingForChildrenTransition(IPersistenceStore persistenceStore,
            IContinuationDispatcher continuationDispatcher,
            IActivityToContinuationConverter activityToContinuationConverter, 
            IRecoverableAction recoverableAction,
            IPrimitiveStatusChanger primitiveStatusChanger)
        {
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (continuationDispatcher == null) throw new ArgumentNullException("continuationDispatcher");
            if (activityToContinuationConverter == null)
                throw new ArgumentNullException("activityToContinuationConverter");
            if (primitiveStatusChanger == null) throw new ArgumentNullException("primitiveStatusChanger");

            _persistenceStore = persistenceStore;
            _continuationDispatcher = continuationDispatcher;
            _activityToContinuationConverter = activityToContinuationConverter;
            _recoverableAction = recoverableAction;
            _primitiveStatusChanger = primitiveStatusChanger;
        }

        public void Transit(Job job, Activity activity)
        {
            if (job == null) throw new ArgumentNullException("job");
            if (activity == null) throw new ArgumentNullException("activity");

            var converted = _activityToContinuationConverter.Convert(activity, job);

            _persistenceStore.Store(converted.Jobs);            
                        
            job.Continuation = converted.Continuation;
            _primitiveStatusChanger.Change<WaitingForChildrenTransition>(job, JobStatus.WaitingForChildren);

            _recoverableAction.Run(() => _continuationDispatcher.Dispatch(job));
        }        
    }
}