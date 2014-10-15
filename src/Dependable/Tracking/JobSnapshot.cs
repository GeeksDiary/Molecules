using System;

namespace Dependable.Tracking
{
    public class JobSnapshot
    {
        public JobSnapshot(Job job)
        {
            Id = job.Id;
            RootId = job.RootId;
            Type = job.Type;
            Method = job.Method;
            Status = job.Status;
            DispatchCount = job.DispatchCount;
        }

        public Guid Id { get; private set; }

        public Guid RootId { get; set; }

        public Type Type { get; private set; }

        public string Method { get; private set; }

        public JobStatus Status { get; private set; }

        public int DispatchCount { get; private set; }
    }
}