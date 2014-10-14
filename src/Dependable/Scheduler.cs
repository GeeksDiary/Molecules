using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dependable.Dispatcher;
using Dependable.Persistence;
using Dependable.Recovery;

namespace Dependable
{
    public interface IScheduler
    {
        Task Start();

        Guid Schedule(Activity activity, Guid? correlationId = null);
    }

    public class Scheduler : IScheduler
    {
        readonly IJobRouter _router;
        readonly IActivityToContinuationConverter _activityToContinuationConverter;
        readonly IEnumerable<IJobPump> _jobPumps;
        readonly QueueConfiguration _queueConfiguration;
        readonly IPersistenceStore _persistenceStore;
        readonly Func<DateTime> _now;
        readonly IFailedJobQueue _failedJobQueue;
        readonly IRecoverableAction _recoverableAction;

        bool _hasStarted;

        public Scheduler(
            QueueConfiguration queueConfiguration,
            IDependableConfiguration configuration,
            IPersistenceStore persistenceStore,
            Func<DateTime> now,
            IFailedJobQueue failedJobQueue,
            IRecoverableAction recoverableAction,
            IJobRouter router,
            IActivityToContinuationConverter activityToContinuationConverter,
            IEnumerable<IJobPump> jobPumps)
        {
            if (queueConfiguration == null) throw new ArgumentNullException("queueConfiguration");
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (now == null) throw new ArgumentNullException("now");
            if (failedJobQueue == null) throw new ArgumentNullException("failedJobQueue");
            if (recoverableAction == null) throw new ArgumentNullException("recoverableAction");
            if (router == null) throw new ArgumentNullException("router");
            if (activityToContinuationConverter == null)
                throw new ArgumentNullException("activityToContinuationConverter");
            if (jobPumps == null) throw new ArgumentNullException("jobPumps");

            _queueConfiguration = queueConfiguration;
            _persistenceStore = persistenceStore;
            _now = now;
            _failedJobQueue = failedJobQueue;
            _recoverableAction = recoverableAction;

            _router = router;
            _activityToContinuationConverter = activityToContinuationConverter;
            _jobPumps = jobPumps;
        }

        public async Task Start()
        {
            if (_hasStarted)
                throw new InvalidOperationException("This scheduler is already started.");
            
            _hasStarted = true;

            _failedJobQueue.Monitor();
            _recoverableAction.Monitor();

            var tasks =_jobPumps.Select(p => p.Start()).ToArray();            
            await Task.WhenAny(tasks);
        }

        public Guid Schedule(Activity activity, Guid? correlationId = null)
        {
            if (activity == null) throw new ArgumentNullException("activity");

            if (correlationId != null)
            {
                var existing = _persistenceStore.LoadBy(correlationId.Value);
                if (existing != null)
                    return existing.Id;
            }
            
            var job = new Job(Guid.NewGuid(), typeof (JobRoot), "Run", new object[0], _now(),
                    correlationId: correlationId,
                    status: JobStatus.WaitingForChildren);

            var converted = _activityToContinuationConverter.Convert(activity, job);

            _persistenceStore.Store(converted.Jobs);

            job.Continuation = converted.Continuation;
            _persistenceStore.Store(job);            

            _router.Route(job);
                
            return job.Id;
        }
    }
}