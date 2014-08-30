using System;

namespace Dependable.Dispatcher
{
    public interface IRunningTransition
    {
        void Transit(Job job);
    }

    public class RunningTransition : IRunningTransition
    {
        readonly IPrimitiveStatusChanger _primitiveStatusChanger;

        public RunningTransition(IPrimitiveStatusChanger primitiveStatusChanger)
        {
            if (primitiveStatusChanger == null) throw new ArgumentNullException("primitiveStatusChanger");
            _primitiveStatusChanger = primitiveStatusChanger;
        }

        public void Transit(Job job)
        {
            job.DispatchCount++;
            _primitiveStatusChanger.Change<RunningTransition>(job, JobStatus.Running);
        }
    }
}