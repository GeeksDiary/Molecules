using System;
using System.Linq;
using Dependable.Dispatcher;
using NSubstitute;

namespace Dependable.Tests
{
    public class JobManagementWrapper
    {
        Job _job;
        readonly World _world;

        public JobManagementWrapper(Job job, World world)
        {
            _job = job;
            _world = world;

            _world.PersistenceStore.Load(job.Id).Returns(_ => _job);
        }

        public JobManagementWrapper AsChildOf(Job parent, JobStatus status = JobStatus.Created)
        {
            _job = Clone(rootId: parent.RootId, parentId: parent.Id);

            var currentAwait = parent.Continuation ?? new Continuation { Children = Enumerable.Empty<Continuation>() };

            parent.Continuation = new Continuation
            {
                ContinueAfterHandlingFailure = currentAwait.ContinueAfterHandlingFailure,
                Children = currentAwait.Children.Concat(new []
                {
                    new Continuation
                    {
                        Id = _job.Id, Status = status
                    }
                })
            };

            return this;
        }

        public JobManagementWrapper In(JobStatus status)
        {
            _job = Clone(status: status);
            return this;
        }

        public JobStatus Status
        {
            get { return _job.Status; }
        }

        public static bool operator ==(Job job, JobManagementWrapper wrapper)
        {
            return wrapper._job == job;
        }

        public static bool operator !=(Job job, JobManagementWrapper wrapper)
        {
            return !(job == wrapper);
        }

        public static bool operator ==(JobManagementWrapper wrapper, Job job)
        {            
            return wrapper._job == job;
        }

        public static bool operator !=(JobManagementWrapper wrapper, Job job)
        {
            return !(wrapper == job);
        }

        public static implicit operator Job(JobManagementWrapper wrapped)
        {
            return wrapped._job;
        }

        public Job Clone(Guid? id = null,
            Type type = null,
            object[] arguments = null,
            DateTime? createdOn = null,
            Guid? rootId = null,
            Guid? parentId = null,
            Guid? correlationId = null,
            JobStatus? status = JobStatus.Ready,
            int? dispatchCount = 0,
            DateTime? retryOn = null)
        {
            return new Job(id ?? _job.Id,
                type ?? _job.Type,
                "Run",
                arguments ?? _job.Arguments,
                createdOn ?? _job.CreatedOn,
                rootId ?? _job.RootId,
                parentId ?? _job.ParentId,
                correlationId ?? _job.CorrelationId,
                status ?? _job.Status,
                dispatchCount ?? _job.DispatchCount,
                retryOn ?? _job.RetryOn);
        }
    }
}