using System;
using System.Collections.Generic;
using System.Linq;

namespace Dependable.Dispatcher
{
    public interface IJobRouter
    {
        void Route(Job job);
        IJobQueue DefaultQueue { get; }
        IDictionary<Type, IJobQueue> SpecificQueues { get; }
    }

    public class JobRouter : IJobRouter
    {
        readonly IJobQueue _defaultQueue;
        readonly IDictionary<Type, IJobQueue> _specificQueues;

        public JobRouter(IDependableConfiguration configuration, Func<ActivityConfiguration, IJobQueue> jobQueueFactory)
        {
            if(configuration == null) throw new ArgumentNullException("configuration");

            _defaultQueue = jobQueueFactory(configuration.DefaultActivityConfiguration);
            _specificQueues = configuration.JobConfigurations.ToDictionary(j => j.JobType, jobQueueFactory);
        }

        public IJobQueue DefaultQueue
        {
            get { return _defaultQueue; }
        }

        public IDictionary<Type, IJobQueue> SpecificQueues
        {
            get { return _specificQueues; }
        }

        public void Route(Job job)
        {
            (SpecificQueues.ContainsKey(job.Type) ? SpecificQueues[job.Type] : DefaultQueue).Write(job);
        }
    }
}