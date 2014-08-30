using System;
using Dependable.Recovery;

namespace Dependable.Dispatcher
{
    public interface IErrorHandlingPolicy
    {
        void RetryOrPoison(Job job, object instance, Exception exception, JobContext context);
    }

    public class ErrorHandlingPolicy : IErrorHandlingPolicy
    {
        readonly IDependableConfiguration _configuration;
        readonly IJobCoordinator _jobCoordinator;
        readonly IMethodBinder _methodBinder;
        readonly IStatusChanger _statusChanger;
        readonly IFailedJobQueue _failedJobQueue;

        public ErrorHandlingPolicy(IDependableConfiguration configuration,
            IJobCoordinator jobCoordinator,
            IMethodBinder methodBinder,
            IStatusChanger statusChanger,
            IFailedJobQueue failedJobQueue)
        {
            if(configuration == null) throw new ArgumentNullException("configuration");
            if(jobCoordinator == null) throw new ArgumentNullException("jobCoordinator");
            if(methodBinder == null) throw new ArgumentNullException("methodBinder");
            if(statusChanger == null) throw new ArgumentNullException("statusChanger");
            if(failedJobQueue == null) throw new ArgumentNullException("failedJobQueue");

            _configuration = configuration;
            _jobCoordinator = jobCoordinator;
            _methodBinder = methodBinder;
            _statusChanger = statusChanger;
            _failedJobQueue = failedJobQueue;
        }

        public void RetryOrPoison(Job job, object instance, Exception exception, JobContext context)
        {
            if(job == null) throw new ArgumentNullException("job");
            if(exception == null) throw new ArgumentNullException("exception");
            if(context == null) throw new ArgumentNullException("context");

            var config = _configuration.For(job.Type);
            if(job.DispatchCount <= config.RetryCount)
            {
                _statusChanger.Change(job, JobStatus.Failed);
                _failedJobQueue.Add(job);
            }
            else
            {
                context.Exception = exception;
                RunPoisonHandler(job, instance, context);                
            }
        }

        void RunPoisonHandler(Job job, object instance, JobContext context)
        {
            _methodBinder.Poison(instance, job.Arguments, context);
            _jobCoordinator.Run(job, () => _statusChanger.Change(job, JobStatus.Poisoned));
        }
    }
}