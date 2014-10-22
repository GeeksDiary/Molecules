using System;
using Dependable.Persistence;
using Dependable.Recovery;

namespace Dependable.Dispatcher
{
    public interface IWaitingForChildrenTransition
    {
        Job Transit(Job job, Activity activity);
    }

    public class WaitingForChildrenTransition : IWaitingForChildrenTransition
    {
        readonly IPersistenceStore _persistenceStore;
        readonly IContinuationDispatcher _continuationDispatcher;
        readonly IActivityToContinuationConverter _activityToContinuationConverter;
        readonly IRecoverableAction _recoverableAction;
        readonly IJobMutator _jobMutator;

        public WaitingForChildrenTransition(IPersistenceStore persistenceStore,
            IContinuationDispatcher continuationDispatcher,
            IActivityToContinuationConverter activityToContinuationConverter,
            IRecoverableAction recoverableAction,
            IJobMutator jobMutator)
        {
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (continuationDispatcher == null) throw new ArgumentNullException("continuationDispatcher");
            if (activityToContinuationConverter == null)
                throw new ArgumentNullException("activityToContinuationConverter");
            if (jobMutator == null) throw new ArgumentNullException("JobMutator");

            _persistenceStore = persistenceStore;
            _continuationDispatcher = continuationDispatcher;
            _activityToContinuationConverter = activityToContinuationConverter;
            _recoverableAction = recoverableAction;
            _jobMutator = jobMutator;
        }

        public Job Transit(Job job, Activity activity)
        {
            if (job == null) throw new ArgumentNullException("job");
            if (activity == null) throw new ArgumentNullException("activity");

            var converted = _activityToContinuationConverter.Convert(activity, job);

            _persistenceStore.Store(converted.Jobs);

            job = _jobMutator.Mutate<WaitingForChildrenTransition>(job, status: JobStatus.WaitingForChildren,
                continuation: converted.Continuation);

            _recoverableAction.Run(() => _continuationDispatcher.Dispatch(job));

            return job;
        }
    }
}