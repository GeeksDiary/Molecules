using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        readonly Dictionary<Guid, ConcurrentQueue<CoordinationRequest>> _coordinationQueues =
            new Dictionary<Guid, ConcurrentQueue<CoordinationRequest>>();

        readonly object _latch = new object();

        public JobCoordinator(IEventStream eventStream, IRecoverableAction recoverableAction)
        {
            if (eventStream == null) throw new ArgumentNullException("eventStream");

            _eventStream = eventStream;
            _recoverableAction = recoverableAction;
        }

        public void Run(Job job, Action action)
        {
            if (job == null) throw new ArgumentNullException("job");

            ConcurrentQueue<CoordinationRequest> q;
            lock (_latch)
            {
                // Creator of the queue always processes it.
                var isProcessing = _coordinationQueues.ContainsKey(job.RootId);
                q = isProcessing ? _coordinationQueues[job.RootId] : new ConcurrentQueue<CoordinationRequest>();

                _coordinationQueues[job.RootId] = q;

                q.Enqueue(new CoordinationRequest {Job = job, Action = action});

                // If this thread did not create the queue, don't process it.
                if (isProcessing) return;
            }

            ProcessAll(job.RootId, q);
        }

        void ProcessAll(Guid id, ConcurrentQueue<CoordinationRequest> q)
        {
            _eventStream.Publish<JobCoordinator>(EventType.Activity,
                EventProperty.ActivityName("CoordinatedEventProcessingCycleStarted"));

            while (true)
            {
                CoordinationRequest request;
                if (q.TryDequeue(out request))
                {
                    _recoverableAction.Run(request.Action, () => Run(request.Job, request.Action));
                }
                else
                {
                    lock (_latch)
                    {
                        if (q.IsEmpty)
                        {
                            _coordinationQueues.Remove(id);
                            break;                            
                        }
                    }
                }                
            }

            _eventStream.Publish<JobCoordinator>(EventType.Activity,
                EventProperty.ActivityName("CoordinatedEventProcessingCycleFinished"));
        }

        class CoordinationRequest
        {
            public Job Job { get; set; }

            public Action Action { get; set; }
        }
    }
}