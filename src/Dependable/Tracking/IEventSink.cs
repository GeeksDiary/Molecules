using System.Collections.Generic;

namespace Dependable.Tracking
{
    public interface IEventSink
    {
        void Dispatch(EventType type, Dictionary<string, object> data);
    }
}