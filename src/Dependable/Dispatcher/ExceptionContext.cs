using System;

namespace Dependable.Dispatcher
{
    public class ExceptionContext
    {
        public Type ActivityType { get; set; }

        public string Method { get; set; }

        public object[] Arguments { get; set; }

        public Exception Exception { get; set; }

        public int DispatchCount { get; set; }
    }
}