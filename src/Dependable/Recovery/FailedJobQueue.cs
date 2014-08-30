using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Dependable.Dispatcher;
using Dependable.Persistence;
using Dependable.Tracking;
using Dependable.Utilities;

namespace Dependable.Recovery
{
    public interface IFailedJobQueue
    {
        void Monitor();

        void Add(Job job);
    }

    public class FailedJobQueue : IFailedJobQueue
    {
        readonly IDependableConfiguration _configuration;
        readonly IPersistenceStore _persistenceStore;
        readonly Func<DateTime> _now;
        readonly IEventStream _eventStream;
        readonly IJobRouter _router;
        readonly Timer _timer;

        readonly ConcurrentBag<Tuple<Guid, DateTime?>> _jobFailures = new ConcurrentBag<Tuple<Guid, DateTime?>>();

        public FailedJobQueue(
            IDependableConfiguration configuration,
            IPersistenceStore persistenceStore,
            Func<DateTime> now,
            IEventStream eventStream,
            IJobRouter router)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (now == null) throw new ArgumentNullException("now");
            if(eventStream == null) throw new ArgumentNullException("eventStream");
            if(router == null) throw new ArgumentNullException("router");

            _configuration = configuration;
            _persistenceStore = persistenceStore;
            _now = now;
            _eventStream = eventStream;
            _router = router;

            _timer = new Timer(OnTick);
        }

        void OnTick(object state)
        {
            var ready = new Collection<Tuple<Guid, DateTime?>>();
            var notReady = new Collection<Tuple<Guid, DateTime?>>();
            
            try
            {                
                Tuple<Guid, DateTime?> item;

                _eventStream.Publish<FailedJobQueue>(
                                                     EventType.TimerActivity,
                                                     EventProperty.ActivityName("RescheduleFailedJobs"),
                                                     EventProperty.Named("FailedItemsQueueLength", _jobFailures.Count));
                
                while (_jobFailures.TryTake(out item))
                {
                    if (item.Item2 < _now())
                        ready.Add(item);
                    else
                        notReady.Add(item);
                }

                foreach(var job in ready.Select(i => _persistenceStore.Load(i.Item1)))
                    _router.Route(job);

                ready.Clear();
            }
            catch (Exception e)
            {
                if (e.IsFatal())
                    throw;

                _eventStream.Publish<FailedJobQueue>(e);
            }
            finally
            {
                foreach(var item in ready.Concat(notReady))
                    _jobFailures.Add(item);

                _timer.Change(_configuration.RetryTimerInterval, Timeout.InfiniteTimeSpan);
            }
        }

        public void Monitor()
        {
            _timer.Change(_configuration.RetryTimerInterval, Timeout.InfiniteTimeSpan);
        }

        public void Add(Job job)
        {
            _eventStream.Publish<FailedJobQueue>(
                                                 EventType.Activity,
                                                 EventProperty.ActivityName("NewFailedItem"),
                                                 EventProperty.JobSnapshot(job));

            _jobFailures.Add(Tuple.Create(job.Id, job.RetryOn));
        }
    }
}