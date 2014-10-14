using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dependable.Tracking;
using Dependable.Utilities;

namespace Dependable.Dispatcher
{
    public interface IJobPump
    {
        Task Start();
    }

    /// <summary>
    /// Processes the jobs in a specified JobQueue.
    /// </summary>
    public class JobPump : IJobPump
    {
        readonly IDispatcher _dispatcher;
        readonly IEventStream _eventStream;
        readonly IJobQueue _queue;
        readonly int _throttle;
        
        int _state;
        
        public JobPump(
            IDispatcher dispatcher,
            IEventStream eventStream,
            IJobQueue queue)
        {
            if(dispatcher == null) throw new ArgumentNullException("dispatcher");
            if(eventStream == null) throw new ArgumentNullException("eventStream");
            if (queue == null) throw new ArgumentNullException("queue");

            _dispatcher = dispatcher;
            _eventStream = eventStream;
            _queue = queue;
            _throttle = queue.Configuration.MaxWorkers == 0 ? int.MaxValue : queue.Configuration.MaxWorkers;            
        }

        public async Task Start()
        {
            if (Interlocked.CompareExchange(ref _state, 1, 0) == 1)
                throw new InvalidOperationException("Cannot start an already started pump.");

            await PumpQueue();            
        }

        async Task PumpQueue()
        {
            var dispatchedList = new List<Task>();
            while (true)
            {
                var job = await _queue.Read();                
                dispatchedList.Add(Dispatch(job, _queue.Configuration));
                if (dispatchedList.Count == _throttle)
                    dispatchedList.Remove(await Task.WhenAny(dispatchedList));
            }
// ReSharper disable once FunctionNeverReturns
        }

        async Task Dispatch(Job job, ActivityConfiguration configuration)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await _dispatcher.Dispatch(job, configuration);
                }
                catch (Exception e)
                {
                    if (e.IsFatal())
                        throw;

                    _eventStream.Publish<JobPump>(e);
                }
            });            
        }
    }
}   