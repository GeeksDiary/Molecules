using System;
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
        readonly IPersistenceStore _persistenceStore;
        readonly Func<DateTime> _now;
        readonly IFailedJobQueue _failedJobQueue;
        readonly IRecoverableAction _recoverableAction;
        readonly IJobPump _jobPump;

        bool _hasStarted;

        public Scheduler(IDependableConfiguration configuration,
            IPersistenceStore persistenceStore,
            Func<DateTime> now,
            IFailedJobQueue failedJobQueue,
            IRecoverableAction recoverableAction,
            IJobPump jobPump,
            IJobRouter router,
            IActivityToContinuationConverter activityToContinuationConverter,
            IJobQueueRecovery jobQueueRecovery)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (now == null) throw new ArgumentNullException("now");
            if (failedJobQueue == null) throw new ArgumentNullException("failedJobQueue");
            if (recoverableAction == null) throw new ArgumentNullException("recoverableAction");
            if (jobPump == null) throw new ArgumentNullException("jobPump");
            if (router == null) throw new ArgumentNullException("router");
            if (activityToContinuationConverter == null)
                throw new ArgumentNullException("activityToContinuationConverter");
            if (jobQueueRecovery == null) throw new ArgumentNullException("jobQueueRecovery");

            _persistenceStore = persistenceStore;
            _now = now;
            _failedJobQueue = failedJobQueue;
            _recoverableAction = recoverableAction;
            _jobPump = jobPump;

            _router = router;
            _activityToContinuationConverter = activityToContinuationConverter;

            jobQueueRecovery.Recover();
        }

        public async Task Start()
        {
            if (_hasStarted)
                throw new InvalidOperationException("This scheduler is already started.");
            
            _hasStarted = true;

            _failedJobQueue.Monitor();
            _recoverableAction.Monitor();

            var tasks = _router.SpecificQueues.Values.Select(q => _jobPump.Start(q)).ToList();
            tasks.Add(_jobPump.Start(_router.DefaultQueue));

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

            _recoverableAction.Run(() => _router.Route(job));

            return job.Id;
        }
    }
}