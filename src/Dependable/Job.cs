using System;
using System.Collections.Generic;
using Dependable.Dispatcher;

namespace Dependable
{
    public class Job
    {
        public Job(
            Guid id,
            Type type,
            string method,
            object[] arguments,
            DateTime createdOn,
            Guid? rootId = null,
            Guid? parentId = null,
            Guid? correlationId = null,                        
            JobStatus status = JobStatus.Ready,
            int dispatchCount = 0,
            DateTime? retryOn = null,
            ExceptionFilter[] exceptionFilters = null)
        {
            if(type == null) throw new ArgumentNullException("type");
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("A valid method is required.");

            Method = method;
            Id = id;
            Type = type;
            Arguments = arguments;
            CreatedOn = createdOn;

            RootId = rootId ?? id;
            ParentId = parentId;
            CorrelationId = correlationId ?? id;
            Status = status;            
            DispatchCount = dispatchCount;
            RetryOn = retryOn;

            Properties = new Dictionary<string, object>();

            ExceptionFilters = exceptionFilters ?? new ExceptionFilter[0];
        }

        public Guid Id { get; private set; }

        public Type Type { get; private set; }

        public string Method { get; private set; }

        public object[] Arguments { get; private set; }

        public DateTime CreatedOn { get; private set; }
        
        public Guid RootId { get; private set; }

        public Guid? ParentId { get; private set; }

        public Guid CorrelationId { get; private set; }

        public JobStatus Status { get; set; }

        public int DispatchCount { get; set; }

        public DateTime? RetryOn { get; set; }

        public Continuation Continuation { get; set; }

        public bool Suspended { get; set; }

        public IDictionary<string, object> Properties { get; private set; }

        public ExceptionFilter[] ExceptionFilters { get; private set; }   

        public override string ToString()
        {
            return string.Format(
                                 "Id: {0}, Type: {1}, DispatchCount: {2}, Status: {3}, Root: {4}, CorrelationId: {5}",
                                 Id,
                                 Type,
                                 DispatchCount,
                                 Status,
                                 RootId,
                                 CorrelationId);
        }
    }
}