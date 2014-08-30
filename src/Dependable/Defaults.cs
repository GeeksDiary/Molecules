using System;

namespace Dependable
{
    public static class Defaults
    {
        public static TimeSpan RetryTimerInterval = TimeSpan.FromMinutes(1);
        public const int MaxQueueLength = 1000;
    }
}