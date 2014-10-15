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
        readonly Queue<Job> _items;

        readonly object _queueAccess = new object();

        TaskCompletionSource<Job> _reader;

        int _suspendedCount;
        readonly IEnumerable<ActivityConfiguration> _allActivityConfiguration;

        public JobQueue(IEnumerable<Job> items, 
            int suspendedCount,
            ActivityConfiguration configuration,    
            IEnumerable<ActivityConfiguration> allActivityConfiguration,
            IPersistenceStore persistenceStore,
            IEventStream eventStream)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (allActivityConfiguration == null) throw new ArgumentNullException("allActivityConfiguration");
            if (persistenceStore == null) throw new ArgumentNullException("persistenceStore");
            if (eventStream == null) throw new ArgumentNullException("eventStream");

            Configuration = configuration;

            _suspendedCount = suspendedCount;
            _allActivityConfiguration = allActivityConfiguration;
            _persistenceStore = persistenceStore;
            _eventStream = eventStream;
            _items = new Queue<Job>(items);
        }

        public ActivityConfiguration Configuration { get; set; }
        
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
                    var max = Configuration.MaxQueueLength;

                    var items = 
                        (Configuration.Type != null ?
                        _persistenceStore.LoadSuspended(Configuration.Type, max) :
                        _persistenceStore.LoadSuspended(_allActivityConfiguration.Select(c => c.Type), max))
                        .ToArray();

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
                if ((_suspendedCount > 0 || _items.Count >= Configuration.MaxQueueLength))
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