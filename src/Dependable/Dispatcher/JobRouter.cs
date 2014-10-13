using System;
using Dependable.Recovery;

namespace Dependable.Dispatcher
{
    public interface IJobRouter
    {
        void Route(Job job);        
    }

    public class JobRouter : IJobRouter
    {
        readonly QueueConfiguration _configuration;
        readonly IRecoverableAction _recoverableAction;

        public JobRouter(QueueConfiguration configuration, IRecoverableAction recoverableAction)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (recoverableAction == null) throw new ArgumentNullException("recoverableAction");
            _configuration = configuration;
            _recoverableAction = recoverableAction;
        }

        public void Route(Job job)
        {
            var queue = _configuration.ActivitySpecificQueues.ContainsKey(job.Type)
                ? _configuration.ActivitySpecificQueues[job.Type]
                : _configuration.Default;

            _recoverableAction.Run(() => queue.Write(job));
        }
    }
}