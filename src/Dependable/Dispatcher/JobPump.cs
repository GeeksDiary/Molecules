using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dependable.Tracking;
using Dependable.Utilities;

namespace Dependable.Dispatcher
{
    public interface IJobPump
    {
        Task Start(IJobQueue queue);
    }

    /// <summary>
    /// Processes the jobs in a specified JobQueue.
    /// </summary>
    public class JobPump : IJobPump
    {
        readonly IDispatcher _dispatcher;
        readonly IEventStream _eventStream;

        public JobPump(
            IDispatcher dispatcher,
            IEventStream eventStream)
        {
            if(dispatcher == null) throw new ArgumentNullException("dispatcher");
            if(eventStream == null) throw new ArgumentNullException("eventStream");

            _dispatcher = dispatcher;
            _eventStream = eventStream;
        }

        public async Task Start(IJobQueue queue)
        {
            var active = new List<Task>();
            var throttle = queue.Configuration.MaxWorkers == 0 ? int.MaxValue : queue.Configuration.MaxWorkers;

            while(true)
            {
                try
                {
                    var jd = await queue.Read();
                    active.Add(Task.Run(() => _dispatcher.Dispatch(jd, queue.Configuration)));

                    if (active.Count == throttle)
                        active.Remove(await Task.WhenAny(active));
                }
                catch(Exception e)
                {
                    if(e.IsFatal())
                        throw;

                    _eventStream.Publish<JobPump>(e);
                }
            }
        }
    }
}   