using System;
using System.Collections.Generic;

namespace Dependable.Recovery
{
    public class RecoveredQueueState
    {
        public RecoveredQueueState(IEnumerable<Job> jobs, int suspendedCount)
        {
            if(jobs == null) throw new ArgumentNullException("jobs");

            Jobs = jobs;
            SuspendedCount = suspendedCount;
        }

        public IEnumerable<Job> Jobs { get; private set; }

        public int SuspendedCount { get; private set; }
    }
}