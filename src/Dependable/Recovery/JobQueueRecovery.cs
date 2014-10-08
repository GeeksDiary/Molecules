using System;
using System.Linq;
using Dependable.Dispatcher;
using Dependable.Persistence;

namespace Dependable.Recovery
{
    public interface IJobQueueRecovery
    {
        void Recover();
    }

    public class JobQueueRecovery : IJobQueueRecovery
    {
        readonly IPersistenceStore _persistenceStore;
        readonly IJobRouter _router;

        public JobQueueRecovery(IPersistenceStore persistenceStore, IJobRouter router)
        {
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (router == null) throw new ArgumentNullException("router");

            _persistenceStore = persistenceStore;
            _router = router;
        }

        public void Recover()
        {
            var readyItems = _persistenceStore.LoadBy(JobStatus.Ready).ToArray();
            var runningItems = _persistenceStore.LoadBy(JobStatus.Running).ToArray();
            var failedItems = _persistenceStore.LoadBy(JobStatus.Failed).ToArray();
            var waitingForChildren = _persistenceStore.LoadBy(JobStatus.WaitingForChildren).ToArray();

            var partiallyCompletedItems = 
                _persistenceStore.LoadBy(JobStatus.ReadyToComplete)
                .Concat(_persistenceStore.LoadBy(JobStatus.ReadyToPoison)).ToArray();

            var all =
                partiallyCompletedItems.Concat(readyItems)
                    .Concat(failedItems)
                    .Concat(waitingForChildren)
                    .Concat(runningItems)
                    .ToArray();

            all = _router.SpecificQueues.Values.Aggregate(all, (current, q) => q.Initialize(current));
            _router.DefaultQueue.Initialize(all);
        }
    }
}