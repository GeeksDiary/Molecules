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

            var availableJob = new TaskCompletionSource<Dependable.Job>();
            var waitForJob = new TaskCompletionSource<Dependable.Job>();
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

        public class MaxWorkers
        {
            readonly World _world = new World();
            readonly Queue<TaskCompletionSource<object>> _dispatchInvocations = new Queue<TaskCompletionSource<object>>();
            readonly Queue<TaskCompletionSource<object>> _dispatchResults = new Queue<TaskCompletionSource<object>>();
            readonly Queue<TaskCompletionSource<object>> _readInvocations = new Queue<TaskCompletionSource<object>>();
            readonly IJobQueue _queue;

            public MaxWorkers()
            {
                var readTask = Task.FromResult((Dependable.Job)_world.NewJob);                
                
                _queue = Substitute.For<IJobQueue>();
                _queue.Configuration.Returns(new ActivityConfiguration().WithMaxWorkers(1));
                
                _queue.Read().Returns(c =>
                {
                    if (_readInvocations.Count != 0)
                        _readInvocations.Dequeue().SetResult(new object());
                    return readTask;
                });

                _world.Dispatcher.Dispatch(null, null).ReturnsForAnyArgs(c =>
                {
                    _dispatchInvocations.Dequeue().SetResult(new object());
                    return _dispatchResults.Dequeue().Task;
                });
            }

            [Fact]
            public async Task ShouldNotReadTheQueueUntilDispatcherCompletes()
            {
                var dispatchInvoked = new TaskCompletionSource<object>();
                _dispatchInvocations.Enqueue(dispatchInvoked);

                _dispatchResults.Enqueue(new TaskCompletionSource<object>());

                var pump = _world.NewJobPump(_queue);
// ReSharper disable once CSharpWarnings::CS4014
                pump.Start();
                
                await dispatchInvoked.Task;

// ReSharper disable once CSharpWarnings::CS4014
                _queue.Received(1).Read();
            }

            [Fact]
            public async Task ShouldContinueWhenDispatcherCompletes()
            {
                var firstDispatchResult = new TaskCompletionSource<object>();
                var secondDispatchResult = new TaskCompletionSource<object>();
                _dispatchResults.Enqueue(firstDispatchResult);
                _dispatchResults.Enqueue(secondDispatchResult);

                var firstDispatchInvoked = new TaskCompletionSource<object>();
                var secondDispatchInvoked = new TaskCompletionSource<object>();
                _dispatchInvocations.Enqueue(firstDispatchInvoked);
                _dispatchInvocations.Enqueue(secondDispatchInvoked);

                var pump = _world.NewJobPump(_queue);
                // ReSharper disable once CSharpWarnings::CS4014
                pump.Start();

                // Wait for first dispatch call to arrive in dispatcher
                await firstDispatchInvoked.Task;

                // Finishing first dispatch should return control back to pump.
                firstDispatchResult.SetResult(new object());

                // Now wait for second dispatch call to arrive in dispatcher
                await secondDispatchInvoked.Task;

                // ReSharper disable once CSharpWarnings::CS4014
                _queue.Received(2).Read();
            }
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