using System;
using System.Linq;
using Dependable.Persistence;

namespace Dependable.Recovery
{
    public interface IJobQueueRecovery
    {
        RecoveredQueueState Recover(Type type);
    }

    public class JobQueueRecovery : IJobQueueRecovery
    {
        readonly IPersistenceStore _persistenceStore;

        public JobQueueRecovery(IPersistenceStore persistenceStore)
        {
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            _persistenceStore = persistenceStore;
        }

        public RecoveredQueueState Recover(Type type)
        {
            var readyItems = _persistenceStore.LoadBy(JobStatus.Ready).ToArray();
            var runningItems = _persistenceStore.LoadBy(JobStatus.Running).ToArray();
            var failedItems = _persistenceStore.LoadBy(JobStatus.Failed).ToArray();
            var waitingForChildren = _persistenceStore.LoadBy(JobStatus.WaitingForChildren).ToArray();

            var partiallyCompletedItems = 
                _persistenceStore.LoadBy(JobStatus.ReadyToComplete)
                .Concat(_persistenceStore.LoadBy(JobStatus.ReadyToPoison)).ToArray();

            var suspendedCount = type == null ? 0 : _persistenceStore.CountSuspended(type);

            return
                new RecoveredQueueState(
                    partiallyCompletedItems.Concat(readyItems).Concat(failedItems).Concat(waitingForChildren).Concat(runningItems).Where(j => j.Type == (type ?? j.Type)),
                    suspendedCount);
        }
    }
}