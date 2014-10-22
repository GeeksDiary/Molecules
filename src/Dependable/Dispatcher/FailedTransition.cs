using System;

namespace Dependable.Dispatcher
{
    public interface IFailedTransition
    {
        Job Transit(Job job);
    }

    public class FailedTransition : IFailedTransition
    {
        readonly IDependableConfiguration _configuration;
        readonly IJobMutator _jobMutator;
        readonly Func<DateTime> _now;

        public FailedTransition(IDependableConfiguration configuration, IJobMutator jobMutator, Func<DateTime> now)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (jobMutator == null) throw new ArgumentNullException("JobMutator");
            if (now == null) throw new ArgumentNullException("now");

            _configuration = configuration;
            _jobMutator = jobMutator;
            _now = now;
        }

        public Job Transit(Job job)
        {
            return _jobMutator.Mutate<FailedTransition>(job, status: JobStatus.Failed,
                retryOn: _now() + _configuration.For(job.Type).RetryDelay);
        }
    }
}