using System;

namespace Dependable
{
    public class JobContext
    {
        public JobContext(Job job)
        {
            if(job == null) throw new ArgumentNullException("job");

            ActivityType = job.Type;
            Method = job.Method;
            Arguments = job.Arguments;
            Id = job.Id;
            CorrelationId = job.CorrelationId;
            DispatchCount = job.DispatchCount;
        }

        public Type ActivityType { get; set; }

        public string Method { get; set; }

        public object[] Arguments { get; set; }

        public Guid Id { get; private set; }

        public Guid CorrelationId { get; private set; }
        
        public int DispatchCount { get; private set; }
        
        public Exception Exception { get; internal set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, Dispatch Count: {1}, Correlation Id: {2}", Id, DispatchCount, CorrelationId);
        }
    }
}