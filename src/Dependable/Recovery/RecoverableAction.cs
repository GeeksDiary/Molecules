using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dependable.Tracking;
using Dependable.Utilities;

namespace Dependable.Recovery
{
    public interface IRecoverableAction
    {
        void Monitor();

        void Run(Action action, Action recoveryAction = null, Action then = null);
    }

    public class RecoverableAction : IRecoverableAction
    {
        readonly IDependableConfiguration _configuration;        
        readonly IEventStream _eventStream;
        readonly ConcurrentQueue<Action> _coordinationFailures = new ConcurrentQueue<Action>();
        readonly Timer _timer;

        public RecoverableAction(IDependableConfiguration configuration, IEventStream eventStream)
        {
            if(configuration == null) throw new ArgumentNullException("configuration");
            if(eventStream == null) throw new ArgumentNullException("eventStream");

            _configuration = configuration;
            _eventStream = eventStream;
            _timer = new Timer(OnTick);
        }

        void OnTick(object state)
        {
            try
            {                
                var maxItemsToRetry = _coordinationFailures.Count;

                _eventStream.Publish<RecoverableAction>(
                                                        EventType.TimerActivity,
                                                        EventProperty.ActivityName("RecoveryTimer"),
                                                        new KeyValuePair<string, object>(
                                                            "NumberOfItemsToProcess",
                                                            maxItemsToRetry));
                
                for (var i = 0; i < maxItemsToRetry; i++)
                {
                    Action request;
                    if (!_coordinationFailures.TryDequeue(out request))
                        break;

                    Task.Run(() => Run(request));
                }
            }
            catch(Exception e)
            {
                if(e.IsFatal())
                    throw;

                _eventStream.Publish<RecoverableAction>(e);
            }
            finally
            {
                _timer.Change(_configuration.RetryTimerInterval, Timeout.InfiniteTimeSpan);
            }
        }

        public void Monitor()
        {
            _timer.Change(_configuration.RetryTimerInterval, Timeout.InfiniteTimeSpan);
        }

        public void Run(Action action, Action recoveryAction = null, Action then = null)
        {
            RunCore(action, recoveryAction ?? action, then);
        }

        void RunCore(Action action, Action recoveryAction = null, Action then = null)
        {
            try
            {
                action();
            }
            catch(Exception e)
            {
                if(e.IsFatal())
                    throw;

                _coordinationFailures.Enqueue(recoveryAction);

                _eventStream.Publish<RecoverableAction>(e);
            }

            if (then != null)
                then();
        }

    }
}