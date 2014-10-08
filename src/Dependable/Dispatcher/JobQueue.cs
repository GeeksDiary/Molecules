using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dependable.Persistence;
using Dependable.Tracking;
using Dependable.Utilities;

namespace Dependable.Dispatcher
{
    public interface IJobQueue
    {
        Task<Job> Read();

        void Write(Job job);

        ActivityConfiguration Configuration { get; set; }
        
        Job[] Initialize(Job[] recoverableJobs);
    }

    /// <summary>
    /// In memory queue for storing jobs.
    /// Assumes there's only one reader and multiple/concurrent
    /// writers.
    /// </summary>
    public class JobQueue : IJobQueue
    {
        readonly IPersistenceStore _persistenceStore;
        readonly IEventStream _eventStream;
        readonly Queue<Job> _items = new Queue<Job>();

        readonly object _queueAccess = new object();

        TaskCompletionSource<Job> _reader;

        int _suspendedCount;
        bool _initialized; 

        public JobQueue(
            ActivityConfiguration configuration,
            IPersistenceStore persistenceStore,
            IEventStream eventStream)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (eventStream == null) throw new ArgumentNullException("eventStream");

            Configuration = configuration;
            _persistenceStore = persistenceStore;
            _eventStream = eventStream;            
        }

        public ActivityConfiguration Configuration { get; set; }

        public Job[] Initialize(Job[] recoverableJobs)
        {
            if (recoverableJobs == null) throw new ArgumentNullException("recoverableJobs");

            if (_initialized)
                throw new InvalidOperationException("Queue is already initialized.");

            if(Configuration.JobType != null)
                _suspendedCount = _persistenceStore.CountSuspended(Configuration.JobType);

            var allMatchingItems = recoverableJobs.Where(j => j.Type == (Configuration.JobType ?? j.Type)).ToArray();

            var consumableItems = (Configuration.JobType != null
                ? allMatchingItems.Take(Configuration.MaxQueueLength)
                : allMatchingItems);

            foreach (var job in consumableItems)            
                _items.Enqueue(job);

            _initialized = true;

            _eventStream.Publish<JobQueue>(
                EventType.Activity,
                EventProperty.ActivityName("JobQueueRecovered"),
                EventProperty.Named("JobCount", _items.Count),
                EventProperty.Named("SuspendedCount", _suspendedCount));

            return recoverableJobs.Except(allMatchingItems).ToArray();
        }

        public async Task<Job> Read()
        {
            var job = await ReadOne();

            if (job != null)
                return job;

            var suspended = await LoadSuspended();

            lock (_queueAccess)
            {
                _suspendedCount -= suspended.Length;

                foreach (var item in suspended)
                    _items.Enqueue(item);

                return _items.Dequeue();
            }
        }

        async Task<Job[]> LoadSuspended()
        {
            var list = new List<Job>();

            while (list.Count == 0)
            {
                try
                {
                    var items =
                        _persistenceStore.LoadSuspended(Configuration.JobType, Configuration.MaxQueueLength).ToArray();

                    _eventStream.Publish<JobQueue>(EventType.Activity,
                        EventProperty.ActivityName("LoadSuspendedItemsStarted"),
                        EventProperty.Named("NumberOfItems", items.Length));

                    foreach (var item in items)
                    {
                        item.Suspended = false;
                        _persistenceStore.Store(item);
                        list.Add(item);
                    }
                }
                catch (Exception e)
                {
                    if (e.IsFatal())
                        throw;

                    _eventStream.Publish<JobQueue>(e);
                }

                if (!list.Any())
                    await Task.Delay(Configuration.RetryDelay);
            }

            _eventStream.Publish<JobQueue>(EventType.Activity,
                EventProperty.ActivityName("LoadSuspendedItemsFinished"),
                EventProperty.Named("NumberOfItems", list.Count));

            return list.ToArray();
        }

        async Task<Job> ReadOne()
        {
            lock (_queueAccess)
            {
                if (_items.Any())
                    return _items.Dequeue();

                if (_suspendedCount != 0)
                    return null;

                //Cancel the old reader first.
                //This will gracefully handle the situation where Read is called multiple times
                //without awaiting for the read to complete.
                if (_reader != null)
                    _reader.TrySetCanceled();

                _reader = new TaskCompletionSource<Job>();
            }

            return await _reader.Task;
        }

        public void Write(Job job)
        {            
            job.Suspended = false;

            var suspendedCount = 0;

            lock (_queueAccess)
            {
                if (_reader != null && _reader.TrySetResult(job))
                    return;

                // Suspend if this is not the default job queue and this is currently overlfowed.
                if (Configuration.JobType != null &&
                    (_suspendedCount > 0 || _items.Count >= Configuration.MaxQueueLength))
                {
                    _suspendedCount++;
                    job.Suspended = true;
                    suspendedCount = _suspendedCount;
                }
                else
                    _items.Enqueue(job);
            }

            if (!job.Suspended)
                return;

            _persistenceStore.Store(job);
            
            _eventStream.Publish<JobQueue>(EventType.JobSuspended, EventProperty.JobSnapshot(job),
                EventProperty.Named("SuspendedCount", suspendedCount));
        }
    }
}