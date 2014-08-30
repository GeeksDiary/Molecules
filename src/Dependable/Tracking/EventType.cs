using System;

namespace Dependable.Tracking
{
    [Flags]
    public enum EventType
    {
        Exception = 1,
        JobStatusChanged = 2,
        JobStatusChangeRejected = 4,
        JobSuspended = 8,
        JobAbandoned = 16,
        Activity = 32,
        TimerActivity = 64
    }
}