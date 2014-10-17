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
        readonly IContinuationLiveness _continuationLiveness;
        readonly IJobCoordinator _coordinator;
        readonly IPrimitiveStatusChanger _primitiveStatusChanger;

        public WaitingForChildrenTransition(IPersistenceStore persistenceStore,
            IContinuationDispatcher continuationDispatcher,
            IActivityToContinuationConverter activityToContinuationConverter, 
            IRecoverableAction recoverableAction,
            IContinuationLiveness continuationLiveness,
            IJobCoordinator coordinator,
            IPrimitiveStatusChanger primitiveStatusChanger)
        {
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (continuationDispatcher == null) throw new ArgumentNullException("continuationDispatcher");
            if (activityToContinuationConverter == null)
                throw new ArgumentNullException("activityToContinuationConverter");
            if (continuationLiveness == null) throw new ArgumentNullException("continuationLiveness");
            if (primitiveStatusChanger == null) throw new ArgumentNullException("primitiveStatusChanger");

            _persistenceStore = persistenceStore;
            _continuationDispatcher = continuationDispatcher;
            _activityToContinuationConverter = activityToContinuationConverter;
            _recoverableAction = recoverableAction;
            _continuationLiveness = continuationLiveness;
            _coordinator = coordinator;
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

            _recoverableAction.Run(
                () => _continuationDispatcher.Dispatch(job, converted.Jobs), 
                () => _coordinator.Run(job, () => _continuationLiveness.Verify(job.Id)));
        }        
    }
}