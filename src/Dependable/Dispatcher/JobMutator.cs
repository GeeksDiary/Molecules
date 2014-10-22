using System;
using Dependable.Persistence;
using Dependable.Tracking;

namespace Dependable.Dispatcher
{
    public interface IJobMutator
    {
        Job Mutate<TSource>(Job job,
            JobStatus? status = null,
            int? dispatchCount = null,
            DateTime? retryOn = null,
            Continuation continuation = null,
            bool? suspended = null);
    }

    public class JobMutator : IJobMutator
    {
        readonly IEventStream _eventStream;
        readonly IPersistenceStore _repository;

        public JobMutator(IEventStream eventStream, IPersistenceStore repository)
        {
            if (eventStream == null) throw new ArgumentNullException("eventStream");
            if (repository == null) throw new ArgumentNullException("repository");
            _eventStream = eventStream;
            _repository = repository;
        }

        public Job Mutate<TSource>(Job job,
            JobStatus? status = null,
            int? dispatchCount = null,
            DateTime? retryOn = null,
            Continuation continuation = null,
            bool? suspended = null)
        {
            var newJob = new Job(job.Id,
                job.Type, 
                job.Method, 
                job.Arguments, 
                job.CreatedOn, 
                job.RootId, 
                job.ParentId,
                job.CorrelationId, 
                status ?? job.Status, 
                dispatchCount ?? job.DispatchCount, 
                retryOn ?? job.RetryOn,
                job.ExceptionFilters, 
                continuation ?? job.Continuation, 
                suspended ?? job.Suspended);

            _repository.Store(newJob);

            if (job.Status != newJob.Status)
            {
                _eventStream.Publish<TSource>(
                    EventType.JobStatusChanged,
                    EventProperty.JobSnapshot(job),
                    EventProperty.FromStatus(job.Status),
                    EventProperty.ToStatus(newJob.Status));
            }

            return newJob;
        }
    }
}