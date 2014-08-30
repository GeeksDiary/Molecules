using System;
using System.Collections.Generic;

namespace Dependable.Tracking
{
    public class ConsoleLoggerEventSink : IEventSink
    {
        readonly EventType _eventMask;
        readonly DefaultTextEventFormatter _formatter = new DefaultTextEventFormatter();

        public ConsoleLoggerEventSink(EventType eventMask)
        {
            _eventMask = eventMask;
        }

        public void Dispatch(EventType type, Dictionary<string, object> data)
        {            
            if ((type & _eventMask) == type)
                Console.Write(_formatter.Format(type, data));
        }
    }

    public static class ConsoleLoggerEventSinkExtensions
    {
        const EventType All = EventType.Exception |
                              EventType.JobStatusChanged | 
                              EventType.JobStatusChangeRejected |
                              EventType.JobSuspended | 
                              EventType.JobAbandoned | 
                              EventType.Activity |
                              EventType.TimerActivity;

        public static DependableConfiguration UseConsoleEventLogger(
            this DependableConfiguration configuration,
            EventType eventMask = All)
        {
            return configuration.UseEventSink(new ConsoleLoggerEventSink(eventMask));
        }
    }
}