using System;

namespace Dependable.Dispatcher
{
    public interface IJobRouter
    {
        void Route(Job job);        
    }

    public class JobRouter : IJobRouter
    {
        readonly QueueConfiguration _configuration;

        public JobRouter(QueueConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _configuration = configuration;
        }

        public void Route(Job job)
        {
            (_configuration.ActivitySpecificQueues.ContainsKey(job.Type)
                ? _configuration.ActivitySpecificQueues[job.Type]
                : _configuration.Default)
                .Write(job);
        }
    }
}