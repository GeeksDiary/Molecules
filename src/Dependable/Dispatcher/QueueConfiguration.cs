using System;
using System.Collections.Generic;

namespace Dependable.Dispatcher
{
    public class QueueConfiguration
    {
        public QueueConfiguration(IJobQueue @default, IDictionary<Type, IJobQueue> activitySpecificQueues)
        {
            if (@default == null) throw new ArgumentNullException("default");
            if (activitySpecificQueues == null) throw new ArgumentNullException("activitySpecificQueues");

            Default = @default;
            ActivitySpecificQueues = activitySpecificQueues;
        }

        public IJobQueue Default { get; private set; }

        public IDictionary<Type, IJobQueue> ActivitySpecificQueues { get; private set; }
    }
}