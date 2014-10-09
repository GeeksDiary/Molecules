using System;

namespace Dependable
{
    public static class Defaults
    {
        public static TimeSpan RetryTimerInterval = TimeSpan.FromMinutes(1);
        public static TimeSpan RetryDelay = TimeSpan.Zero;
        public const int RetryCount = 0;
        public const int MaxWorkers = 0;        
        public const int MaxQueueLength = 1000;
    }
}