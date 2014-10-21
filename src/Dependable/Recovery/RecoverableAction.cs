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

        readonly ConcurrentQueue<RecoverableActionRequest> _itemsToRecover =
            new ConcurrentQueue<RecoverableActionRequest>();

        readonly Timer _timer;

        public RecoverableAction(IDependableConfiguration configuration, IEventStream eventStream)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (eventStream == null) throw new ArgumentNullException("eventStream");

            _configuration = configuration;
            _eventStream = eventStream;
            _timer = new Timer(OnTick);
        }

        void OnTick(object state)
        {
            try
            {
                var maxItemsToRetry = _itemsToRecover.Count;

                _eventStream.Publish<RecoverableAction>(
                    EventType.TimerActivity,
                    EventProperty.ActivityName("RecoveryTimer"),
                    new KeyValuePair<string, object>(
                        "NumberOfItemsToProcess",
                        maxItemsToRetry));

                for (var i = 0; i < maxItemsToRetry; i++)
                {
                    RecoverableActionRequest request;
                    if (!_itemsToRecover.TryDequeue(out request))
                        break;

                    Task.Run(() => RunCore(request)).FailFastOnException();
                }
            }
            catch (Exception e)
            {
                if (e.IsFatal())
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
            RunCore(new RecoverableActionRequest
            {
                Action = action,
                RecoveryAction = recoveryAction ?? action,
                Then = then
            });
        }

        void RunCore(RecoverableActionRequest request)
        {
            var success = false;
            try
            {
                request.Action();
                success = true;
            }
            catch (Exception e)
            {
                if (e.IsFatal())
                    throw;

                _itemsToRecover.Enqueue(new RecoverableActionRequest
                {
                    Action = request.RecoveryAction,
                    RecoveryAction = request.RecoveryAction,
                    Then = request.Then
                });

                _eventStream.Publish<RecoverableAction>(e);
            }

            if (success && request.Then != null)
                request.Then();
        }

        public class RecoverableActionRequest
        {
            public Action Action { get; set; }

            public Action RecoveryAction { get; set; }

            public Action Then { get; set; }
        }
    }
}