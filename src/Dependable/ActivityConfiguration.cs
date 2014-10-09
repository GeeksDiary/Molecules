using System;

namespace Dependable
{
    public class ActivityConfiguration
    {
        public ActivityConfiguration(Type type = null, ActivityConfiguration @default = null)
        {   
            Type = type;

            if (@default == null)
            {
                MaxWorkers = Defaults.MaxWorkers;
                RetryDelay = Defaults.RetryDelay;
                RetryCount = Defaults.RetryCount;
                MaxQueueLength = Defaults.MaxQueueLength;
            }
            else
            {
                MaxWorkers = @default.MaxWorkers;
                RetryDelay = @default.RetryDelay;
                RetryCount = @default.RetryCount;
                MaxQueueLength = @default.MaxQueueLength;    
            }                                       
        }

        public Type Type { get; private set; }

        public int MaxWorkers { get; private set; }

        public int MaxQueueLength { get; private set; }

        public TimeSpan RetryDelay { get; private set; }

        public int RetryCount { get; private set; }

        public ActivityConfiguration WithMaxWorkers(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count", count, "Count must be greater than 0.");

            MaxWorkers = count;
            return this;
        }

        public ActivityConfiguration WithMaxQueueLength(int length)
        {
            if(length <= 0)
                throw new ArgumentOutOfRangeException("length", length, "Length must be greater than 0.");

            MaxQueueLength = length;
            return this;
        }

        public ActivityConfiguration WithRetryDelay(TimeSpan retryDelay)
        {
            RetryDelay = retryDelay;
            return this;
        }

        public ActivityConfiguration WithRetryCount(int of)
        {
            RetryCount = of;
            return this;
        }
    }
}