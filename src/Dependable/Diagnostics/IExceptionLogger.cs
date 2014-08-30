using System;

namespace Dependable.Diagnostics
{
    public interface IExceptionLogger
    {
        void Log(Exception exception);
    }

    class NullExceptionLogger : IExceptionLogger
    {
        public void Log(Exception exception)
        {
        }
    }
}