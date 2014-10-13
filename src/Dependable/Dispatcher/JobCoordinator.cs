using System;
using System.Collections.Concurrent;
using Dependable.Recovery;
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
        readonly IRecoverableAction _recoverableAction;

        readonly ConcurrentDictionary<Guid, CoordinationQueue> _coordinationQueues =
            new ConcurrentDictionary<Guid, CoordinationQueue>();

        public JobCoordinator(IEventStream eventStream, IRecoverableAction recoverableAction)
        {
            if (eventStream == null) throw new ArgumentNullException("eventStream");

            _eventStream = eventStream;
            _recoverableAction = recoverableAction;
        }

        public void Run(Job job, Action action)
        {
            if (job == null) throw new ArgumentNullException("job");

            var q = _coordinationQueues.GetOrAdd(job.RootId, j => new CoordinationQueue());

            q.Operations.Enqueue(new CoordinationRequest { Job = job, Action = action });
            
            /*
             * After we queue the action, 
             * we take a lock on the queue to see 
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

            CoordinationRequest request;
            while (queue.Operations.TryDequeue(out request))
            {
                var currentRequest = request;
                _recoverableAction.Run(request.Action, () => Run(currentRequest.Job, currentRequest.Action));
            }

            lock (queue)
                queue.IsProcessing = false;

            _eventStream.Publish<JobCoordinator>(EventType.Activity,
                EventProperty.ActivityName("CoordinatedEventProcessingCycleFinished"));
        }

        class CoordinationQueue
        {
            public CoordinationQueue()
            {
                Operations = new ConcurrentQueue<CoordinationRequest>();
            }

            public ConcurrentQueue<CoordinationRequest> Operations { get; private set; }

            public bool IsProcessing { get; set; }
        }

        class CoordinationRequest
        {
            public Job Job { get; set; }

            public Action Action { get; set; }
        }
    }
}