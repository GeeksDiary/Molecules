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

        public JobManagementWrapper AsChildOf(ref Job parent, JobStatus status = JobStatus.Created)
        {
            _job = Clone(rootId: parent.RootId, parentId: parent.Id);

            var currentContinuation = parent.Continuation ?? new Continuation { Children = Enumerable.Empty<Continuation>() };

            var newContinuation = new Continuation
            {
                ContinueAfterHandlingFailure = currentContinuation.ContinueAfterHandlingFailure,
                Children = currentContinuation.Children.Concat(new[]
                {
                    new Continuation
                    {
                        Id = _job.Id,
                        Status = status
                    }
                })
            };

            parent = new Job(parent.Id, parent.Type, parent.Method, parent.Arguments, parent.CreatedOn, parent.RootId,
                parent.ParentId, parent.CorrelationId, parent.Status, parent.DispatchCount, parent.RetryOn,
                parent.ExceptionFilters, newContinuation, parent.Suspended);

            _world.PersistenceStore.Load(parent.Id).Returns(parent);
            return this;
        }

        public JobManagementWrapper In(JobStatus status)
        {
            _job = Clone(status: status);
            return this;
        }

        public JobManagementWrapper OfType<T>()
        {
            _job = Clone(type: typeof (T));
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
                rootId: rootId ?? _job.RootId,
                parentId: parentId ?? _job.ParentId,
                correlationId: correlationId ?? _job.CorrelationId,
                status: status ?? _job.Status,
                dispatchCount: dispatchCount ?? _job.DispatchCount,
                retryOn: retryOn ?? _job.RetryOn);
        }
    }
}