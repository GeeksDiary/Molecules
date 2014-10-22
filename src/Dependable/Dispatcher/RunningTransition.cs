using System;

namespace Dependable.Dispatcher
{
    public interface IRunningTransition
    {
        Job Transit(Job job);
    }

    public class RunningTransition : IRunningTransition
    {
        readonly IJobMutator _jobMutator;

        public RunningTransition(IJobMutator jobMutator)
        {
            if (jobMutator == null) throw new ArgumentNullException("JobMutator");
            _jobMutator = jobMutator;
        }

        public Job Transit(Job job)
        {
            return _jobMutator.Mutate<RunningTransition>(job, 
                status: JobStatus.Running, 
                dispatchCount: job.DispatchCount + 1);
        }
    }
}