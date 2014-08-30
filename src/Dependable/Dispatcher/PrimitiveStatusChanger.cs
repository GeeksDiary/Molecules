using System;
using Dependable.Persistence;
using Dependable.Tracking;

namespace Dependable.Dispatcher
{
    public interface IPrimitiveStatusChanger
    {
        JobStatus Change<TSource>(Job job, JobStatus to);
    }

    public class PrimitiveStatusChanger : IPrimitiveStatusChanger
    {
        readonly IEventStream _eventStream;
        readonly IPersistenceStore _repository;

        public PrimitiveStatusChanger(IEventStream eventStream, IPersistenceStore repository)
        {
            if (eventStream == null) throw new ArgumentNullException("eventStream");
            if (repository == null) throw new ArgumentNullException("repository");
            _eventStream = eventStream;
            _repository = repository;
        }

        public JobStatus Change<TSource>(Job job, JobStatus to)
        {
            var oldStatus = job.Status;
            job.Status = to;
            _repository.Store(job);

            _eventStream.Publish<TSource>(
                    EventType.JobStatusChanged,
                    EventProperty.JobSnapshot(job),
                    EventProperty.FromStatus(oldStatus),
                    EventProperty.ToStatus(job.Status));
            
            return oldStatus;
        }
    }
}