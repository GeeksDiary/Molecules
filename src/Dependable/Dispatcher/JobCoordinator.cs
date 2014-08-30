using System;
using System.Collections.Concurrent;
using Dependable.Tracking;

namespace Dependable.Dispatcher
{
    public interface IJobCoordinator
    {
        void Run(Job job, Action action);
    }

    public class JobCoordinator : IJobCoordinator
    {
        readonly IEventStream _eventStream;

        readonly ConcurrentDictionary<Guid, CoordinationQueue> _coordinationQueues =
            new ConcurrentDictionary<Guid, CoordinationQueue>();

        public JobCoordinator(IEventStream eventStream)
        {
            if (eventStream == null) throw new ArgumentNullException("eventStream");

            _eventStream = eventStream;
        }

        public void Run(Job job, Action action)
        {
            if (job == null) throw new ArgumentNullException("job");

            var q = _coordinationQueues.GetOrAdd(job.RootId, j => new CoordinationQueue());

            q.Operations.Enqueue(action);
            
            /*
             * After queue the action, we take a lock on the queue to see 
             * if this thread should process the queue.             
             */
            lock (q)
            {
                if (q.IsProcessing)
                    return;
                q.IsProcessing = true;
            }

            ProcessAll(q);
        }

        void ProcessAll(CoordinationQueue queue)
        {
            _eventStream.Publish<JobCoordinator>(EventType.Activity,
                EventProperty.ActivityName("CoordinatedEventProcessingCycleStarted"));

            Action request;
            while (queue.Operations.TryDequeue(out request))
                request();

            lock (queue)
                queue.IsProcessing = false;

            _eventStream.Publish<JobCoordinator>(EventType.Activity,
                EventProperty.ActivityName("CoordinatedEventProcessingCycleFinished"));
        }

        class CoordinationQueue
        {
            public CoordinationQueue()
            {
                Operations = new ConcurrentQueue<Action>();
            }

            public ConcurrentQueue<Action> Operations { get; private set; }

            public bool IsProcessing { get; set; }
        }
    }
}