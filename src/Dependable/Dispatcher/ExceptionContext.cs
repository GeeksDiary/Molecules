using System;

namespace Dependable.Dispatcher
{
    public class ExceptionContext
    {
        public Exception Exception { get; set; }

        public int DispatchCount { get; set; }
    }
}