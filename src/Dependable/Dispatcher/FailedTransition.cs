using System;

namespace Dependable.Dispatcher
{
    public interface IFailedTransition
    {
        void Transit(Job job);
    }

    public class FailedTransition : IFailedTransition
    {
        readonly IDependableConfiguration _configuration;
        readonly IPrimitiveStatusChanger _primitiveStatusChanger;
        readonly Func<DateTime> _now;

        public FailedTransition(IDependableConfiguration configuration, IPrimitiveStatusChanger primitiveStatusChanger, Func<DateTime> now)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (primitiveStatusChanger == null) throw new ArgumentNullException("primitiveStatusChanger");
            if (now == null) throw new ArgumentNullException("now");

            _configuration = configuration;
            _primitiveStatusChanger = primitiveStatusChanger;
            _now = now;
        }

        public void Transit(Job job)
        {            
            job.RetryOn = _now() + _configuration.For(job.Type).RetryDelay;
            _primitiveStatusChanger.Change<FailedTransition>(job, JobStatus.Failed);
        }
    }
}