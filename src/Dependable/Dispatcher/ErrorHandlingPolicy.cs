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
        readonly IStatusChanger _statusChanger;
        readonly IFailedJobQueue _failedJobQueue;
        readonly IRecoverableAction _recoverableAction;

        public ErrorHandlingPolicy(IDependableConfiguration configuration,
            IJobCoordinator jobCoordinator,
            IStatusChanger statusChanger,
            IFailedJobQueue failedJobQueue,
            IRecoverableAction recoverableAction)
        {
            if(configuration == null) throw new ArgumentNullException("configuration");
            if(jobCoordinator == null) throw new ArgumentNullException("jobCoordinator");
            if(statusChanger == null) throw new ArgumentNullException("statusChanger");
            if(failedJobQueue == null) throw new ArgumentNullException("failedJobQueue");
            if (recoverableAction == null) throw new ArgumentNullException("recoverableAction");

            _configuration = configuration;
            _jobCoordinator = jobCoordinator;
            _statusChanger = statusChanger;
            _failedJobQueue = failedJobQueue;
            _recoverableAction = recoverableAction;
        }

        public void RetryOrPoison(Job job, object instance, Exception exception, JobContext context)
        {
            if(job == null) throw new ArgumentNullException("job");
            if(exception == null) throw new ArgumentNullException("exception");
            if(context == null) throw new ArgumentNullException("context");

            var config = _configuration.For(job.Type);
            if(job.DispatchCount <= config.RetryCount)
            {
                _recoverableAction.Run(() => _statusChanger.Change(job, JobStatus.Failed),
                    then: () => _failedJobQueue.Add(job));
            }
            else
            {
                context.Exception = exception;
                _jobCoordinator.Run(job, () => _statusChanger.Change(job, JobStatus.Poisoned));
            }
        }
    }
}