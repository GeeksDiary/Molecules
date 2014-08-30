using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dependable.Diagnostics;
using Dependable.Utilities;

namespace Dependable.Tracking
{
    public interface IEventStream
    {
        void Publish<T>(Exception exception, params KeyValuePair<string, object>[] properties);
        void Publish<T>(EventType type, params KeyValuePair<string, object>[] properties);
    }

    public class EventStream : IEventStream
    {
        readonly IEnumerable<IEventSink> _sinks;
        readonly IExceptionLogger _exceptionLogger;
        readonly Func<DateTime> _now;

        public EventStream(IEnumerable<IEventSink> sinks, IExceptionLogger exceptionLogger, Func<DateTime> now)
        {
            if(sinks == null) throw new ArgumentNullException("sinks");
            if(exceptionLogger == null) throw new ArgumentNullException("exceptionLogger");
            if(now == null) throw new ArgumentNullException("now");

            _sinks = sinks;
            _exceptionLogger = exceptionLogger;
            _now = now;
        }

        public void Publish<T>(Exception exception, params KeyValuePair<string, object>[] properties)
        {
            _exceptionLogger.Log(exception);
            Publish<T>(EventType.Exception, new KeyValuePair<string, object>("Exception", exception));
        }

        public void Publish<T>(EventType type, params KeyValuePair<string, object>[] properties)
        {
            var data = properties.ToDictionary(i => i.Key, i => i.Value);
            Publish<T>(type, data);
        }

        void Publish<T>(EventType type, Dictionary<string, object> data)
        {
            data["Source"] = typeof(T);
            data["EventTime"] = _now().ToUniversalTime();
            data["ThreadId"] = Thread.CurrentThread.ManagedThreadId;
            
            foreach(var sink in _sinks)
                DispatchAndHandleException(sink, type, data);
        }

        void DispatchAndHandleException(IEventSink sink, EventType type, Dictionary<string, object> data)
        {
            try
            {
                sink.Dispatch(type, data);
            }
            catch(Exception e)
            {
                if(e.IsFatal())
                    throw;

                _exceptionLogger.Log(e);
            }
        }
    }
}