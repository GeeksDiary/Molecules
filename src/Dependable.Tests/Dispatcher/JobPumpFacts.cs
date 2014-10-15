using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dependable.Dispatcher;
using NSubstitute;
using Xunit;

namespace Dependable.Tests.Dispatcher
{
    public class JobPumpFacts
    {
        readonly World _world = new World();

        [Fact]
        public async Task ShouldInvokeDispatcherWithLastJobReadFromQueue()
        {
            var job = _world.NewJob.In(JobStatus.Ready);

            var availableJob = new TaskCompletionSource<Job>();
            var waitForJob = new TaskCompletionSource<Job>();
            var items = new Queue<Task>(new [] { availableJob.Task, waitForJob.Task });
            
            var queue = Substitute.For<IJobQueue>();
            queue.Read().Returns(c => items.Dequeue());
            queue.Configuration = new ActivityConfiguration();

            var dispatch = new TaskCompletionSource<object>();
            _world.Dispatcher.WhenForAnyArgs(d => d.Dispatch(null, null)).Do(c => dispatch.SetResult(new object()));

            var pump = _world.NewJobPump(queue);

// ReSharper disable once CSharpWarnings::CS4014
            pump.Start();

            availableJob.SetResult(job);

            await dispatch.Task;

// ReSharper disable once CSharpWarnings::CS4014
            _world.Dispatcher.Received(1).Dispatch(job, queue.Configuration);
        }
    }

    public static partial class WorldExtensions
    {
        public static JobPump NewJobPump(this World world, IJobQueue queue)
        {
            if (world == null) throw new ArgumentNullException("world");
            if (queue == null) throw new ArgumentNullException("queue");

            return new JobPump(world.Dispatcher, world.EventStream, queue);
        }
    }
}